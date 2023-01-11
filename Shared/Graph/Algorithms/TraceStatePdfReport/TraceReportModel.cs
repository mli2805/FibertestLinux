namespace Fibertest.Graph;

public class TraceReportModel
{
    public string? TraceTitle;
    public string? TraceState;
    public string? RtuTitle;
    public string? RtuSoftwareVersion;
    public string? PortTitle;
    public string? MeasurementTimestamp;
    public string? RegistrationTimestamp;

    public List<AccidentLineModel>? Accidents;
}