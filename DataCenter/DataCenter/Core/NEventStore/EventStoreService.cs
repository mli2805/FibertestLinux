using Fibertest.Graph;
using Fibertest.Utils;
using NEventStore;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public class EventStoreService
    {
        const string Timestamp = @"Timestamp";
        private readonly IWritableOptions<DataCenterConfig> _config;
        private readonly ILogger<EventStoreService> _logger;
        private readonly IEventStoreInitializer _eventStoreInitializer;
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

        public EventStoreService(IWritableOptions<DataCenterConfig> config, ILogger<EventStoreService> logger, IEventStoreInitializer eventStoreInitializer,
            SnapshotRepository snapshotRepository, EventLogComposer eventLogComposer,
             CommandAggregator commandAggregator, EventsQueue eventsQueue, Model writeModel)
        {
            _eventsPortion = config.Value.EventSourcingPortion; // default = 100
            _config = config;
            _logger = logger;
            _eventStoreInitializer = eventStoreInitializer;
            _snapshotRepository = snapshotRepository;
            _eventLogComposer = eventLogComposer;
            _commandAggregator = commandAggregator;
            _eventsQueue = eventsQueue;
            _writeModel = writeModel;
        }

        public async Task<int> Init()
        {
            StoreEvents = _eventStoreInitializer.Init();

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

            var flag = false;
            if (LastEventNumberInSnapshot == 0 && eventStream.CommittedEvents.FirstOrDefault() == null)
            {
                flag = true;
                foreach (object seed in DbSeeds.Collection)
                    await SendCommand(seed, "developer", "OnServer");

                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "Empty graph is seeded with default zone and users.");
            }

            if (!_writeModel.TceTypeStructs.Any())
            {
                var cmd = new ReSeedTceTypeStructList() { TceTypes = TceTypeStructExt.Generate().ToList() };
                await SendCommand(cmd, "developer", "OnServer");
            }

            var eventMessages = eventStream.CommittedEvents.ToList();
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"{eventMessages.Count} events should be applied...");
            foreach (var eventMessage in eventMessages)
            {
                _writeModel.Apply(eventMessage.Body);
                _eventLogComposer.AddEventToLog(eventMessage);
            }
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "Events applied successfully.");
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Last event number is {LastEventNumberInSnapshot + eventMessages.Count}");

            var msg = eventStream.CommittedEvents.LastOrDefault();
            if (msg != null)
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $@"Last applied event has timestamp {msg.Headers[Timestamp]:O}");


            if (!flag) // this block should be deleted after MGTS, Mogilev and Vitebsk update to 986+
            {
                var previousStartOnVersion = _config.Value.PreviousStartOnVersion ?? "";
                if (previousStartOnVersion.IsOlder("2.1.0.986"))
                {
                    foreach (var user in _writeModel.Users)
                    {
                        var cmd = new UpdateUser()
                        {
                            UserId = user.UserId,
                            Title = user.Title,
                            Role = user.Role,
                            Email = user.Email,
                            Sms = user.Sms,
                            EncodedPassword = UserExt.FlipFlop(user.EncodedPassword).GetSha256(),
                            ZoneId = user.ZoneId,
                        };
                        var _ = await SendCommand(cmd, "system", "OnServer");
                    }
                }
            }

            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"{_writeModel.Rtus.Count} RTU found");

            return eventMessages.Count;
        }

        // especially for Migrator.exe
        public Task<int> SendCommands(List<object> cmds, string username, string clientIp)
        {
            foreach (var cmd in cmds)
            {
                var result = _commandAggregator.Validate(cmd);
                if (!string.IsNullOrEmpty(result))
                    _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), result);
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
                return new string[0];
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), e.Message);
                return new string[0];
            }
        }
    }
}