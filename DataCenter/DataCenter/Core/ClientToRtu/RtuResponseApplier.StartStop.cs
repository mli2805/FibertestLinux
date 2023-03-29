using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public partial class RtuResponseApplier
    {
        public async Task<string> ApplyMonitoringSettingsResult(ApplyMonitoringSettingsDto dto, string jsonResult)
        {
            var result = Deserialize<RequestAnswer>(jsonResult);
            if (result.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
            {
                var cmd = new ChangeMonitoringSettings
                {
                    RtuId = dto.RtuId,
                    PreciseMeas = dto.Timespans.PreciseMeas.GetFrequency(),
                    PreciseSave = dto.Timespans.PreciseSave.GetFrequency(),
                    FastSave = dto.Timespans.FastSave.GetFrequency(),
                    TracesInMonitoringCycle = dto.Ports.Select(p => p.TraceId).ToList(),
                    IsMonitoringOn = dto.IsMonitoringOn,
                };

                var esResult = await _responseToEventSourcing.SendToEventSourcing(dto.ClientConnectionId, cmd);
                if (esResult != null)
                    jsonResult = result.TurnInto(ReturnCode.Error, esResult).SerializeToJson();

            }
            else
                _logger.Error(Logs.DataCenter, "Failed to apply monitoring settings!");

            return jsonResult;
        }

        public async Task<string> ApplyStopMonitoringResult(StopMonitoringDto dto, string jsonResult)
        {
            var result = Deserialize<RequestAnswer>(jsonResult);
            if (result.ReturnCode == ReturnCode.Ok)
            {
                var cmd = new StopMonitoring() { RtuId = dto.RtuId };
                var esResult = await _responseToEventSourcing.SendToEventSourcing(dto.ClientConnectionId, cmd);
                if (esResult != null)
                    jsonResult = result.TurnInto(ReturnCode.Error, esResult).SerializeToJson();

            }
            else
                _logger.Error(Logs.DataCenter, "Failed to stop monitoring!");

            return jsonResult;

        }
    }
}
