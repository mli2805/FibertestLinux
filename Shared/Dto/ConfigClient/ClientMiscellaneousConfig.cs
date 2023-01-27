namespace Fibertest.Dto;

public class ClientMiscellaneousConfig
{
    public string PathToSor { get; set; } = string.Empty;
    public string RtuDefualtIp { get; set; } = "192.168.96.";
    public GpsInputMode GpsInputMode { get; set; } = GpsInputMode.DegreesMinutesAndSeconds;
    public GraphVisibilityLevel GraphVisibilityLevel { get; set; } = GraphVisibilityLevel.AllDetails;
    public bool DoNotSignalAboutSuspicion { get; set; } = false;
    public int MaxCableReserve { get; set; } = 200;
    public int MeasurementTimeoutMs { get; set; } = 60000;
    public int VeexLineParamsTimeoutMs { get; set; } = 2000;
}