namespace Fibertest.Dto;

public class MapConfig
{

    public int Zoom { get; set; } = 15;
    public int MaxZoom { get; set; } = 21;
    public int SaveMaxZoomNoMoreThan { get; set; } = 15;
    public double CenterLatitude { get; set; } = 57;
    public double CenterLongitude { get; set; } = 29.5;

    public bool IsHighDensityGraph { get; set; } = true;
    public int ThresholdZoom { get; set; } = 16;
    public double ScreenPartAsMargin { get; set; } = 0.1;

    public AccessModeFt MapAccessMode { get; set; } = AccessModeFt.ServerAndCache;
    public string GMapProvider { get; set; } = @"OpenStreetMap";
}