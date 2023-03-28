using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public static class MonitoringSettingsDtoFactory
    {
        public static ApplyMonitoringSettingsDto CreateDto(this MonitoringSettingsModel model)
        {
            return new ApplyMonitoringSettingsDto(model.RtuId, model.RtuMaker)
            {
                OtdrId = model.OtdrId,
                MainVeexOtau = model.MainVeexOtau,
                IsMonitoringOn = model.IsMonitoringOn,
                Timespans = model.CreateFrequenciesDto(),
                // Ports = ask explicitly ! 
            };
        }

        public static ApplyMonitoringSettingsDto AddPortList(this ApplyMonitoringSettingsDto dto,
            MonitoringSettingsModel model)
        {
            dto.Ports = model.CreatePortWithTraceList();
            return dto;
        }

        public static List<PortWithTraceDto> CreatePortWithTraceList(this MonitoringSettingsModel model)
        {
            var ports = new List<PortWithTraceDto>();
            foreach (var charon in model.Charons)
            {
                foreach (var port in charon.Ports.Where(p => p.IsIncluded))
                {
                    ports.Add(
                        new PortWithTraceDto(new OtauPortDto(port.PortNumber, charon.IsMainCharon)
                        {
                            Serial = charon.Serial,
                            OtauId = charon.OtauId,
                            MainCharonPort = charon.MainCharonPort,
                        }, port.TraceId)
                        );
                }
            }
            return ports;
        }

        private static MonitoringTimespansDto CreateFrequenciesDto(this MonitoringSettingsModel model)
        {
            return new MonitoringTimespansDto
            {
                PreciseMeas = model.Frequencies.SelectedPreciseMeasFreq.GetTimeSpan(),
                PreciseSave = model.Frequencies.SelectedPreciseSaveFreq.GetTimeSpan(),
                FastSave = model.Frequencies.SelectedFastSaveFreq.GetTimeSpan(),
            };
        }


        public static ChangeMonitoringSettings CreateCommand(this ApplyMonitoringSettingsDto dto)
        {
            var cmd = new ChangeMonitoringSettings()
            {
                RtuId = dto.RtuId,
                PreciseMeas = dto.Timespans.PreciseMeas.GetFrequency(),
                PreciseSave = dto.Timespans.PreciseSave.GetFrequency(),
                FastSave = dto.Timespans.FastSave.GetFrequency(),
                TracesInMonitoringCycle = dto.Ports.Select(p => p.TraceId).ToList(),
                IsMonitoringOn = dto.IsMonitoringOn,
            };
            return cmd;
        }
    }
}
