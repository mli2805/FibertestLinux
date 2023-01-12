using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using NEventStore;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class EventStoreService
{
    const string Timestamp = @"Timestamp";
    private readonly IWritableOptions<MysqlConfig> _configMySql;
    private readonly ILogger<EventStoreService> _logger;
    private readonly IDbInitializer _dbInitializer;
    private readonly SnapshotRepository _snapshotRepository;
    private readonly EventLogComposer _eventLogComposer;
    public IStoreEvents? StoreEvents;
    private readonly CommandAggregator _commandAggregator;
    private readonly EventsQueue _eventsQueue;
    private readonly Model _writeModel;

    public Guid StreamIdOriginal;

    private readonly int _eventsPortion;
    public int LastEventNumberInSnapshot;
    public DateTime LastEventDateInSnapshot;

    private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public EventStoreService(IWritableOptions<MysqlConfig> configMySql, IWritableOptions<EventSourcingConfig> configEvent,
        ILogger<EventStoreService> logger,
        IDbInitializer dbInitializer,
        SnapshotRepository snapshotRepository, EventLogComposer eventLogComposer,
        CommandAggregator commandAggregator, EventsQueue eventsQueue, Model writeModel)
    {
        _eventsPortion = configEvent.Value.EventSourcingPortion;
        _configMySql = configMySql;
        _logger = logger;
        _dbInitializer = dbInitializer;
        _snapshotRepository = snapshotRepository;
        _eventLogComposer = eventLogComposer;
        _commandAggregator = commandAggregator;
        _eventsQueue = eventsQueue;
        _writeModel = writeModel;
    }


    public async Task InitializeBothDb()
    {
        var resetDb = _configMySql.Value.ResetDb; // default = false
        if (resetDb)
        {
            _logger.LogInfo(Logs.DataCenter, "ResetDb flag is TRUE! DB will be deleted...");
            await using (var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions))
            {
                await dbContext.Database.EnsureDeletedAsync();
            }
            _dbInitializer.DropDatabase();
            _configMySql.Update(o => o.ResetDb = false);
            _logger.LogInfo(Logs.DataCenter, "Db deleted successfully.");
        }
        else
            StreamIdOriginal = _dbInitializer.GetStreamIdIfExists();

        if (StreamIdOriginal != Guid.Empty)
            _logger.LogInfo(Logs.DataCenter, $"Found DB with StreamIdOriginal {StreamIdOriginal}");
        else
        {
            StreamIdOriginal = Guid.NewGuid();
            _logger.LogInfo(Logs.DataCenter, $"DB will be created with StreamIdOriginal {StreamIdOriginal}");
        }

        await using (var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions))
        {
            await dbContext.Database.EnsureCreatedAsync();
            _logger.LogInfo(Logs.DataCenter, $"{_dbInitializer.ConnectionLogLine}");
        }
        var eventCount = await InitializeEventStoreService();
        _logger.LogInfo(Logs.DataCenter, $"Event store service initialization: {eventCount} events");
    }


    private async Task<int> InitializeEventStoreService()
    {
        StoreEvents = _dbInitializer.Init();

        var snapshot = await _snapshotRepository.ReadSnapshotAsync(StreamIdOriginal);
        if (snapshot == null)
        {
            LastEventNumberInSnapshot = 0;
            LastEventDateInSnapshot = DateTime.MinValue;
        }
        else
        {
            LastEventNumberInSnapshot = snapshot.Item1;
            LastEventDateInSnapshot = snapshot.Item3;
            if (!await _writeModel.Deserialize(_logger, snapshot.Item2))
                return -1;
            _eventLogComposer.Initialize();
        }

        var eventStream = StoreEvents.OpenStream(StreamIdOriginal);

        if (LastEventNumberInSnapshot == 0 && eventStream.CommittedEvents.FirstOrDefault() == null)
        {
            foreach (var cmd in DbSeeds.Collection)
                await SendCommand(cmd, "developer", "OnServer");
            _logger.LogInfo(Logs.DataCenter, "Empty graph is seeded with default zone and users.");
        }

        var eventMessages = eventStream.CommittedEvents.ToList();
        _logger.LogInfo(Logs.DataCenter, $"{eventMessages.Count} events should be applied...");
        foreach (var eventMessage in eventMessages)
        {
            _writeModel.Apply(eventMessage.Body);
            _eventLogComposer.AddEventToLog(eventMessage);
        }
        _logger.LogInfo(Logs.DataCenter, "Events applied successfully.");
        _logger.LogInfo(Logs.DataCenter, $"Last event number is {LastEventNumberInSnapshot + eventMessages.Count}");

        var msg = eventStream.CommittedEvents.LastOrDefault();
        if (msg != null)
            _logger.LogInfo(Logs.DataCenter, $@"Last applied event has timestamp {msg.Headers[Timestamp]:O}");

        _logger.LogInfo(Logs.DataCenter, $"{_writeModel.Rtus.Count} RTU found");

        return eventMessages.Count;
    }

    // especially for Migrator.exe
    public Task<int> SendCommands(List<object> cmds, string username, string clientIp)
    {
        foreach (var cmd in cmds)
        {
            var result = _commandAggregator.Validate(cmd);
            if (!string.IsNullOrEmpty(result))
                _logger.LogError(Logs.DataCenter, result);
        }

        StoreEventsInDb(username, clientIp);
        return Task.FromResult(cmds.Count);
    }

    public Task<string?> SendCommand(object cmd, string username, string clientIp)
    {
        // ilya: can pass user id\role as an argument to When to check permissions
        var result = _commandAggregator.Validate(cmd); // Aggregate checks if command is valid
        // and if so, transforms command into event and passes it to WriteModel
        // WriteModel applies event and returns whether event was applied successfully

        if (result == null)                                   // if command was valid and applied successfully it should be persisted
            StoreEventsInDb(username, clientIp);
        return Task.FromResult(result);
    }

    private void StoreEventsInDb(string username, string clientIp)
    {
        var eventStream = StoreEvents.OpenStream(StreamIdOriginal);
        foreach (var e in _eventsQueue.EventsWaitingForCommit)   // takes already applied event(s) from WriteModel's list
        {
            var eventMessage = WrapEvent(e, username, clientIp);
            eventStream.Add(eventMessage);   // and stores this event in BD
            _eventLogComposer.AddEventToLog(eventMessage);
        }

        _eventsQueue.Commit();                                     // now cleans WriteModel's list

        eventStream.CommitChanges(Guid.NewGuid());
    }

    private EventMessage WrapEvent(object e, string username, string clientIp)
    {
        var msg = new EventMessage();
        msg.Headers.Add("Timestamp", DateTime.Now);
        msg.Headers.Add("Username", username);
        msg.Headers.Add("ClientIp", clientIp);
        msg.Headers.Add("VersionId", StreamIdOriginal);
        msg.Body = e;
        return msg;
    }

    public string[] GetEvents(int revision)
    {
        try
        {
            var events = StoreEvents
                .OpenStream(StreamIdOriginal, revision + 1)
                .CommittedEvents
                //     .Select(x => x.Body) // not only Body but Header too
                .Select(x => JsonConvert.SerializeObject(x, JsonSerializerSettings))
                .Take(_eventsPortion) // it depends on tcp buffer size and performance of clients' pc
                .ToArray();
            return events;
        }
        catch (StreamNotFoundException)
        {
            return Array.Empty<string>();
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.DataCenter, e.Message);
            return Array.Empty<string>();
        }
    }
}