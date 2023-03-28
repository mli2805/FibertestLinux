using System.Windows.Media;
using Fibertest.Dto;

namespace Fibertest.WpfClient;

public static class ChannelEventWpfExt
{
    public static Brush GetBrush(this ChannelEvent state, bool isForeground)
    {
        switch (state)
        {
            case ChannelEvent.Broken:
                return Brushes.Red;
            case ChannelEvent.Nothing:
                return isForeground ? Brushes.LightGray : Brushes.Transparent;
            case ChannelEvent.Repaired:
                return isForeground ? Brushes.Black : Brushes.Transparent;
            default:
                return isForeground ? Brushes.Black : Brushes.Transparent;
        }
    }
}