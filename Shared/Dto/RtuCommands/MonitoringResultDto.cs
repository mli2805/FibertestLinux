namespace Fibertest.Dto;

public class MonitoringResultDto
{
    public MeasurementResult MeasurementResult;
    public Guid RtuId;
    public DateTime TimeStamp;
    public PortWithTraceDto PortWithTrace = new();
    public BaseRefType BaseRefType;
    public FiberState TraceState;
    public byte[]? SorBytes;
}