namespace Fibertest.WpfClient
{
    public static class ScreenPartAsMarginProvider
    {
        private static readonly double[] MarginsForHighDensityGraph = 
            { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.2, 0.3, 0.4, 0.6, 1.0};
        private static readonly double[] MarginsForLowDensityGraph =
            { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.2, 0.5, 0.7, 0.9, 1.0, 1.5, 3.0, 3.0, 3.0};

        public static void SetMargin(this CurrentGis currentGis, int zoom)
        {
            currentGis.ScreenPartAsMargin = currentGis.IsHighDensityGraph 
                ? MarginsForHighDensityGraph[zoom-1] 
                : MarginsForLowDensityGraph[zoom-1];
        }
    }
}
