using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.Graph;

public static class MonitoringStateExt
{
    public static string ToLocalizedString(this MonitoringState state)
    {
        switch (state)
        {
            case MonitoringState.Off: return Resources.SID_Manual;
            case MonitoringState.On: return Resources.SID_Automatic;
            case MonitoringState.Unknown :
            default: return Resources.SID_Unknown;
        }
    }

    public static Uri GetPictogramUri(this MonitoringState state)
    {
        switch (state)
        {
            case MonitoringState.Unknown:
                return new Uri(@"pack://application:,,,/Resources/LeftPanel/EmptySquare.png");
            case MonitoringState.Off:
                return new Uri(@"pack://application:,,,/Resources/LeftPanel/GreySquare.png");
            case MonitoringState.On:
                return new Uri(@"pack://application:,,,/Resources/LeftPanel/BlueSquare.png");
            default:
                return new Uri(@"pack://application:,,,/Resources/LeftPanel/EmptySquare.png");
        }
    }
}