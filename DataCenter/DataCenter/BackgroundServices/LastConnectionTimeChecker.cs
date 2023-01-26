using System.Diagnostics;
using AutoMapper;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Microsoft.Extensions.Options;

namespace Fibertest.DataCenter;

public class LastConnectionTimeChecker : BackgroundService
{
    private static readonly IMapper Mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingWebApiProfile>()).CreateMapper();

    private readonly ILogger<LastConnectionTimeChecker> _logger;
    private readonly GlobalState _globalState;
    private readonly EventStoreService _eventStoreService;
    private readonly ClientCollection _clientCollection;
    private readonly RtuStationsRepository _rtuStationsRepository;
    private readonly Model _writeModel;
    private readonly IFtSignalRClient _ftSignalRClient;
    private readonly SmtpNotifier _smtpNotifier;
    // private readonly SmsManager _smsManager;
    private readonly SnmpNotifier _snmpNotifier;
    private readonly TimeSpan _checkHeartbeatEvery;
    private readonly TimeSpan _rtuHeartbeatPermittedGap;
    private readonly TimeSpan _clientHeartbeatPermittedGap;

    public LastConnectionTimeChecker(IOptions<DataCenterConfig> fullConfig, ILogger<LastConnectionTimeChecker> logger, 
        GlobalState globalState, EventStoreService eventStoreService, ClientCollection clientCollection, 
        RtuStationsRepository rtuStationsRepository, Model writeModel,
        IFtSignalRClient ftSignalRClient,
        SmtpNotifier smtpNotifier, 
        // SmsManager smsManager, 
        SnmpNotifier snmpNotifier)
    {
        _logger = logger;
        _globalState = globalState;
        _eventStoreService = eventStoreService;
        _clientCollection = clientCollection;
        _rtuStationsRepository = rtuStationsRepository;
        _writeModel = writeModel;
        _ftSignalRClient = ftSignalRClient;
        _smtpNotifier = smtpNotifier;
        // _smsManager = smsManager;
        _snmpNotifier = snmpNotifier;

        _checkHeartbeatEvery = TimeSpan.FromSeconds(fullConfig.Value.ServerTimeouts.CheckHeartbeatEvery);
        _rtuHeartbeatPermittedGap = TimeSpan.FromSeconds(fullConfig.Value.ServerTimeouts.RtuHeartbeatPermittedGap);
        _clientHeartbeatPermittedGap = TimeSpan.FromSeconds(fullConfig.Value.ServerTimeouts.ClientConnectionsPermittedGap);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;
        _logger.LogInfo(Logs.DataCenter, $"Last connection checker starts. Process {pid}, thread {tid}");
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        // if server just started it should give RTUs time to check-in
        await Task.Delay(_rtuHeartbeatPermittedGap, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_globalState.IsDatacenterInDbOptimizationMode)
                await Tick();
            await Task.Delay(_checkHeartbeatEvery, stoppingToken);
        }
    }

    private async Task Tick()
    {
        var deadClients = _clientCollection.CleanDeadClients(_clientHeartbeatPermittedGap);
        foreach (var deadClient in deadClients)
            await _eventStoreService.SendCommand(new LostClientConnection(), deadClient.Key, deadClient.Value);

        var networkEvents = await GetNewNetworkEvents(_rtuHeartbeatPermittedGap);
        if (networkEvents.Count == 0)
            return;

        foreach (var networkEvent in networkEvents)
        {
            var command = new AddNetworkEvent()
            {
                RtuId = networkEvent.RtuId,
                EventTimestamp = DateTime.Now,
                OnMainChannel = networkEvent.OnMainChannel,
                OnReserveChannel = networkEvent.OnReserveChannel,
            };
            if (!string.IsNullOrEmpty(await _eventStoreService.SendCommand(command, "system", "OnServer")))
                continue;
            var lastEvent = _writeModel.NetworkEvents.LastOrDefault(n => n.RtuId == networkEvent.RtuId);
            if (lastEvent == null) continue;
            var dto = Mapper.Map<NetworkEventDto>(lastEvent);
            await _ftSignalRClient.NotifyAll("AddNetworkEvent", dto.ToCamelCaseJson());
        }
    }

    private async Task<List<NetworkEvent>> GetNewNetworkEvents(TimeSpan rtuHeartbeatPermittedGap)
    {
        DateTime noLaterThan = DateTime.Now - rtuHeartbeatPermittedGap;
        var stations = await _rtuStationsRepository.GetAllRtuStations();

        List<RtuStation> changedStations = new List<RtuStation>();
        List<NetworkEvent> networkEvents = new List<NetworkEvent>();
        foreach (var rtuStation in stations)
        {
            NetworkEvent? networkEvent = await CheckRtuStation(rtuStation, noLaterThan);
            if (networkEvent != null)
            {
                changedStations.Add(rtuStation);
                networkEvents.Add(networkEvent);
                _snmpNotifier.SendRtuNetworkEvent(networkEvent);
                _logger.LogInfo(Logs.DataCenter, $"{networkEvent}");
            }
        }
        if (changedStations.Count > 0)
            await _rtuStationsRepository.SaveAvailabilityChanges(changedStations);

        return networkEvents;
    }

    private async Task<NetworkEvent?> CheckRtuStation(RtuStation rtuStation, DateTime noLaterThan)
    {
        var networkEvent = new NetworkEvent() { RtuId = rtuStation.RtuGuid, EventTimestamp = DateTime.Now };

        var flag = await CheckMainChannel(rtuStation, noLaterThan, networkEvent);

        if (rtuStation.IsReserveAddressSet)
            flag = flag || await CheckReserveChannel(rtuStation, noLaterThan, networkEvent);

        return flag ? networkEvent : null;
    }

    private async Task<bool> CheckReserveChannel(RtuStation rtuStation, DateTime noLaterThan, NetworkEvent networkEvent)
    {
        var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuStation.RtuGuid);
        if (rtu == null) return false;
        var rtuTitle = rtu.Title;
        if (rtuStation.LastConnectionByReserveAddressTimestamp < noLaterThan &&
            rtuStation.IsReserveAddressOkDuePreviousCheck)
        {
            rtuStation.IsReserveAddressOkDuePreviousCheck = false;
            networkEvent.OnReserveChannel = ChannelEvent.Broken;
            _logger.LogInfo(Logs.DataCenter, $"RTU \"{rtuTitle}\" Reserve channel - Broken");
            await _smtpNotifier.SendNetworkEvent(rtuStation.RtuGuid, false, false);
            // _smsManager.SendNetworkEvent(rtuStation.RtuGuid, false, false);
            return true;
        }

        if (rtuStation.LastConnectionByReserveAddressTimestamp >= noLaterThan &&
            !rtuStation.IsReserveAddressOkDuePreviousCheck)
        {
            rtuStation.IsReserveAddressOkDuePreviousCheck = true;
            networkEvent.OnReserveChannel = ChannelEvent.Repaired;
            _logger.LogInfo(Logs.DataCenter, $"RTU \"{rtuTitle}\" Reserve channel - Recovered");
            await _smtpNotifier.SendNetworkEvent(rtuStation.RtuGuid, false, true);
            // _smsManager.SendNetworkEvent(rtuStation.RtuGuid, false, true);
            return true;
        }
        return false;
    }

    private async Task<bool> CheckMainChannel(RtuStation rtuStation, DateTime noLaterThan, NetworkEvent networkEvent)
    {
        var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuStation.RtuGuid);
        if (rtu == null) return false;
        var rtuTitle = rtu.Title;
        if (rtuStation.LastConnectionByMainAddressTimestamp < noLaterThan && rtuStation.IsMainAddressOkDuePreviousCheck)
        {
            rtuStation.IsMainAddressOkDuePreviousCheck = false;
            networkEvent.OnMainChannel = ChannelEvent.Broken;
            _logger.LogInfo(Logs.DataCenter, $"RTU \"{rtuTitle}\" Main channel - Broken");
            await _smtpNotifier.SendNetworkEvent(rtuStation.RtuGuid, true, false);
            // _smsManager.SendNetworkEvent(rtuStation.RtuGuid, true, false);
            return true;
        }

        if (rtuStation.LastConnectionByMainAddressTimestamp >= noLaterThan && !rtuStation.IsMainAddressOkDuePreviousCheck)
        {
            rtuStation.IsMainAddressOkDuePreviousCheck = true;
            networkEvent.OnMainChannel = ChannelEvent.Repaired;
            _logger.LogInfo(Logs.DataCenter, $"RTU \"{rtuTitle}\" Main channel - Recovered");
            await _smtpNotifier.SendNetworkEvent(rtuStation.RtuGuid, true, true);
            // _smsManager.SendNetworkEvent(rtuStation.RtuGuid, true, true);
            return true;
        }
        return false;
    }
}