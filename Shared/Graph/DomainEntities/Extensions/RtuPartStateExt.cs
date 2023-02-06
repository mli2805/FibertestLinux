using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.Graph;

public static class RtuPartStateExt
{
    public static string ToLocalizedString(this RtuPartState state)
    {
        switch (state)
        {
            case RtuPartState.Broken:
                return Resources.SID_Broken;
            case RtuPartState.NotSetYet:
                return "";
            case RtuPartState.Ok:
                return Resources.SID_Ok;
            default: return "";
        }
    }

    public static string GetPathToPictogram(this RtuPartState state)
    {
        switch (state)
        {
            case RtuPartState.Broken:
                return @"pack://application:,,,/Resources/LeftPanel/RedSquare.png";
            case RtuPartState.NotSetYet:
                return @"pack://application:,,,/Resources/LeftPanel/EmptySquare.png";
            case RtuPartState.Ok:
            default:
                return @"pack://application:,,,/Resources/LeftPanel/GreenSquare.png";
        }
    }
}