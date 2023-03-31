using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public class IitRtuMessagesProcessor
    {
        private readonly ILogger<IitRtuMessagesProcessor> _logger;
        private readonly Model _writeModel;
        private readonly EventStoreService _eventStoreService;
        private readonly CommonBopProcessor _commonBopProcessor;
        private readonly MeasurementFactory _measurementFactory;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly SorFileRepository _sorFileRepository;

        public IitRtuMessagesProcessor(ILogger<IitRtuMessagesProcessor> logger, Model writeModel, 
            EventStoreService eventStoreService, CommonBopProcessor commonBopProcessor,
            MeasurementFactory measurementFactory, AccidentLineModelFactory accidentLineModelFactory,
            RtuStationsRepository rtuStationsRepository,
            SorFileRepository sorFileRepository)
        {
            _logger = logger;
            _writeModel = writeModel;
            _eventStoreService = eventStoreService;
            _commonBopProcessor = commonBopProcessor;
            _measurementFactory = measurementFactory;
            _accidentLineModelFactory = accidentLineModelFactory;
            _rtuStationsRepository = rtuStationsRepository;
            _sorFileRepository = sorFileRepository;
        }

        public async Task ProcessBopStateChanges(BopStateChangedDto dto)
        {
            if (await _rtuStationsRepository.IsRtuExist(dto.RtuId))
                await CheckAndSendBopNetworkEventIfNeeded(dto);
        }

        public async Task ProcessMonitoringResult(MonitoringResultDto dto)
        {
            if (!await _rtuStationsRepository.IsRtuExist(dto.RtuId)) return;

            var sorId = await _sorFileRepository.AddSorBytesAsync(dto.SorBytes!);
            if (sorId != -1)
                await SaveEventFromDto(dto, sorId);
        }

        private async Task SaveEventFromDto(MonitoringResultDto dto, int sorId)
        {
            var addMeasurement = _measurementFactory.CreateCommand(dto, sorId);
            var result = await _eventStoreService.SendCommand(addMeasurement, "system", "OnServer");

            if (result != null) // Unknown trace or something else
            {
                await _sorFileRepository.RemoveSorBytesAsync(sorId);
                return;
            }

            //TODO notifications
            // var signal = _writeModel.GetTraceStateDto(_accidentLineModelFactory, sorId);
            // await _ftSignalRClient.NotifyAll("AddMeasurement", signal.ToCamelCaseJson());
            await CheckAndSendBopNetworkIfNeeded(dto);
            //
            // if (addMeasurement.EventStatus > EventStatus.JustMeasurementNotAnEvent && dto.BaseRefType != BaseRefType.Fast)
            // {
            //     var unused = Task.Factory.StartNew(() =>
            //         SendNotificationsAboutTraces(dto, addMeasurement)); // here we do not wait result
            // }
        }

        // BOP - because message about BOP came
        private async Task CheckAndSendBopNetworkEventIfNeeded(BopStateChangedDto dto)
        {
            var otau = _writeModel.Otaus.FirstOrDefault(o =>
                o.NetAddress.Ip4Address == dto.OtauIp && o.NetAddress.Port == dto.TcpPort
            );
            if (otau != null)
            {
                _logger.Info(Logs.DataCenter, 
                    $@"RTU {dto.RtuId.First6()} BOP {otau.NetAddress.ToStringA()
                    } state changed to {dto.IsOk} (because message about BOP came)");
                var cmd = new AddBopNetworkEvent()
                {
                    EventTimestamp = DateTime.Now,
                    RtuId = dto.RtuId,
                    Serial = dto.Serial,
                    OtauIp = otau.NetAddress.Ip4Address,
                    TcpPort = otau.NetAddress.Port,
                    IsOk = dto.IsOk,
                };
                await _commonBopProcessor.PersistBopEvent(cmd);
            }
        }

        // BOP - because message with monitoring result came
        private async Task CheckAndSendBopNetworkIfNeeded(MonitoringResultDto dto)
        {
            var otau = _writeModel.Otaus.FirstOrDefault(o =>
                o.Serial == dto.PortWithTrace.OtauPort.Serial
            );
            if (otau != null && !otau.IsOk)
            {
                _logger.Info(Logs.DataCenter, 
                    $@"RTU {dto.RtuId.First6()} BOP {dto.PortWithTrace.OtauPort.Serial
                    } state changed to OK (because message with monitoring result came)");
                var cmd = new AddBopNetworkEvent()
                {
                    EventTimestamp = DateTime.Now,
                    RtuId = dto.RtuId,
                    Serial = dto.PortWithTrace.OtauPort.Serial!,
                    OtauIp = otau.NetAddress.Ip4Address,
                    TcpPort = otau.NetAddress.Port,
                    IsOk = true,
                };
                await _commonBopProcessor.PersistBopEvent(cmd);
            }
        }
    }
 
}
