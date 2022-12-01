// ReSharper disable InconsistentNaming

namespace Fibertest.Dto;

public class VeexOtdr
{
    public bool canVscout;
    public IList<object>? enabledOptions;
    public string? id;
    public bool isConnected;
    public string? mainframeId;
    public string? opticalModuleSerialNumber;

    public SupportedMeasurementParameters? supportedMeasurementParameters;
    public TcpProxy? tcpProxy;
}

public class SupportedMeasurementParameters
{
    public Dictionary<string, LaserUnit>? laserUnits;
}

public class LaserUnit
{
    public string? connector;
    public Dictionary<string, DistanceRange>? distanceRanges;
    public double dynamicRange;
}

public class DistanceRange
{
    public string[]? averagingTimes;
    public string[]? fastAveragingTimes;
    public string[]? pulseDurations;
    public string[]? resolutions;
}

public class TcpProxy
{
    public string? self;
}