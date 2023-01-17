using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.Graph
{
    public static class RtuOccupationStateExt
    {
        public static string GetLocalized(this RtuOccupationState state)
        {
            switch (state.RtuOccupation)
            {
                case RtuOccupation.InitializeRtu:
                    return Resources.SID_RTU_initialization_in_progress;
                case RtuOccupation.DoAutoBaseMeasurement:
                    return Resources.SID_Auto_base_measurement_in_progress;
                 case RtuOccupation.DoMeasurementClient:
                    return Resources.SID_Measurement__Client__in_progress;
                 case RtuOccupation.ApplyMonitoringSettings:
                    return Resources.SID_Monitoring_settings_are_being_applied;
                default:
                    return @"Unknown occupation state";
            }
        }
    }
}
