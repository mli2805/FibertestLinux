using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Newtonsoft.Json;

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
                {
                    result.ReturnCode = ReturnCode.Error;
                    result.ErrorMessage = esResult;
                    jsonResult = JsonConvert.SerializeObject(result, JsonSerializerSettings);
                }

            }
            else
                _logger.Error(Logs.DataCenter, "Failed to apply monitoring settings!");

            return jsonResult;
        }
    }
}
