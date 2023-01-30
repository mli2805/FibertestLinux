using System.Windows.Media;
using Fibertest.Dto;

namespace Fibertest.WpfClient;

public static class RtuPartStateWpfExt
{
    public static Brush GetBrush(this RtuPartState state, bool isForeground)
    {
        switch (state)
        {
            case RtuPartState.Broken:
                return Brushes.Red;
            case RtuPartState.NotSetYet:
                return isForeground ? Brushes.LightGray : Brushes.Transparent;
            case RtuPartState.Ok:
                return isForeground ? Brushes.Black : Brushes.Transparent;
            default:
                return isForeground ? Brushes.Black : Brushes.Transparent;
        }
    }


}