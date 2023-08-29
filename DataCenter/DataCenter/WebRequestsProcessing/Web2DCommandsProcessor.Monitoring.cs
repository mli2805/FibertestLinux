using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public partial class Web2DCommandsProcessor
    {
        public async Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var response = await _c2RCommandsProcessor.SendCommand(dto);
            return (RequestAnswer)JsonConvert.DeserializeObject(response, JsonSerializerSettings)!;
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var response = await _c2RCommandsProcessor.SendCommand(dto);
            var resultDto = (RequestAnswer)JsonConvert.DeserializeObject(response)!;
            return resultDto.ReturnCode == ReturnCode.Ok;
        }

        public async Task<RtuMonitoringSettingsDto?> GetRtuMonitoringSettings(string username, Guid rtuId)
        {
            _logger.Info(Logs.WebApi, "GetRtuMonitoringSettings");
            await Task.Delay(1);

            var result = new RtuMonitoringSettingsDto();
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return result;
            result.RtuMaker = rtu.RtuMaker;
            result.OtdrId = rtu.OtdrId;
            result.MainVeexOtau = rtu.MainVeexOtau;
            result.RtuTitle = rtu.Title;
            result.MonitoringMode = rtu.MonitoringState;
            result.PreciseMeas = rtu.PreciseMeas;
            result.PreciseSave = rtu.PreciseSave;
            result.FastSave = rtu.FastSave;
            result.Lines = PrepareRtuMonitoringPortLines(rtu);
            return result;
        }

        private List<RtuMonitoringPortDto> PrepareRtuMonitoringPortLines(Graph.Rtu rtu)
        {
            var result = new List<RtuMonitoringPortDto>();
            for (int i = 1; i <= rtu.OwnPortCount; i++)
            {
                if (rtu.Children.ContainsKey(i))
                {
                    var otau = rtu.Children[i];
                    for (int j = 1; j <= otau.OwnPortCount; j++)
                    {
                        var trace = _writeModel.Traces.FirstOrDefault(t =>
                            t.OtauPort != null && t.OtauPort.Serial == otau.Serial && t.OtauPort.OpticalPort == j);
                        result.Add(trace != null
                            ? PrepareRtuMonitoringPortLine(trace, j, $"{i}-")
                            : new RtuMonitoringPortDto() { Port = $"{i}-{j}", PortMonitoringMode = PortMonitoringMode.NoTraceJoined });
                    }
                }
                else
                {
                    var trace = _writeModel.Traces.FirstOrDefault(t =>
                        t.RtuId == rtu.Id && t.Port == i && (t.OtauPort == null || t.OtauPort.IsPortOnMainCharon));
                    result.Add(trace != null
                        ? PrepareRtuMonitoringPortLine(trace, i, "")
                        : new RtuMonitoringPortDto() { Port = i.ToString(), PortMonitoringMode = PortMonitoringMode.NoTraceJoined });
                }
            }
            return result;
        }

        private RtuMonitoringPortDto PrepareRtuMonitoringPortLine(Trace trace, int port, string mainPort)
        {
            var result = new RtuMonitoringPortDto()
            {
                Port = mainPort + port,
                TraceId = trace.TraceId.ToString(),
                OtauPortDto = trace.OtauPort,
                TraceTitle = trace.Title,
                DurationOfFastBase = trace.FastDuration.Seconds,
                DurationOfPreciseBase = trace.PreciseDuration.Seconds,
                PortMonitoringMode = !trace.HasEnoughBaseRefsToPerformMonitoring
                    ? PortMonitoringMode.TraceHasNoBase
                    : !trace.IsIncludedInMonitoringCycle
                        ? PortMonitoringMode.Off
                        : PortMonitoringMode.On,
            };
            return result;
        }


    }
}
