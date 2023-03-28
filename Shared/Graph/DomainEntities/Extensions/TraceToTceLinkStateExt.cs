using Fibertest.Dto;

namespace Fibertest.Graph;

public static class TraceToTceLinkStateExt
{
    public static Uri GetPictogram(this TraceToTceLinkState state)
    {
        switch (state)
        {
            case TraceToTceLinkState.NoLink:
                return new Uri(@"pack://application:,,,/Resources/LeftPanel/EmptySquare.png");
            case TraceToTceLinkState.LinkTceOn:
                return new Uri(@"pack://application:,,,/Resources/LeftPanel/GreenSquare.png");
            case TraceToTceLinkState.LinkTceOff:
                return new Uri(@"pack://application:,,,/Resources/LeftPanel/GreySquare.png");
            default:
                return new Uri(@"pack://application:,,,/Resources/LeftPanel/EmptySquare.png");
        }
    }
}