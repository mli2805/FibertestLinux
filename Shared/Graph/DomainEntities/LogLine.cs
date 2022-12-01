namespace Fibertest.Graph;

public class LogLine
{
    public int Ordinal;
    public string? Username;
    public string? ClientIp;
    public DateTime Timestamp;
    public LogOperationCode OperationCode;
    // public string? OperationName => OperationCode.GetLocalizedString();
    public string? RtuTitle;
    public string? TraceTitle;
    public string? OperationParams;
}