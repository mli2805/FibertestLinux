using System.Windows.Media;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public static class FiberStateWpfExt
    {
        public static Brush GetBrush(this FiberState state, bool isForeground)
        {
            switch (state)
            {
                case FiberState.NotInTrace:
                    return Brushes.Aqua;
                case FiberState.NotJoined:
                case FiberState.DistanceMeasurement:
                    return Brushes.Blue;

                case FiberState.NotInZone:
                    return Brushes.LightGray;
                case FiberState.Unknown:
                case FiberState.Ok:
                    return isForeground ? Brushes.Black : Brushes.Transparent;
                case FiberState.Suspicion:
                    return Brushes.Yellow;
                case FiberState.Minor:
                    return isForeground 
                        ?  new SolidColorBrush(Color.FromArgb(255, 164, 128, 224)) 
                        : new SolidColorBrush(Color.FromArgb(255, 128, 128, 192));
                case FiberState.Major:
                    return isForeground 
                        ? Brushes.Fuchsia // FF00FF
                        :  Brushes.HotPink; // FF69B4
                case FiberState.User:
                    return Brushes.Green;
                case FiberState.Critical:
                case FiberState.FiberBreak:
                case FiberState.NoFiber:
                    return Brushes.Red;
                case FiberState.HighLighted:
                    // return new SolidColorBrush(Color.FromArgb(255, 195, 0, 255));
                    return Brushes.Lime;
                default:
                    return Brushes.Black;
            }
        }

    }
}
