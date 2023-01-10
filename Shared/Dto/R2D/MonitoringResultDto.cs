namespace Fibertest.Dto;

public class MonitoringResultDto
{
    public MeasurementResult MeasurementResult;
    public Guid RtuId;
    public DateTime TimeStamp;
    public PortWithTraceDto PortWithTrace;
    public BaseRefType BaseRefType;
    public FiberState TraceState;
    public byte[]? SorBytes;

    public MonitoringResultDto(MeasurementResult measurementResult, Guid rtuId, 
        DateTime timeStamp, PortWithTraceDto portWithTrace, BaseRefType baseRefType, FiberState traceState)
    {
        MeasurementResult = measurementResult;
        RtuId = rtuId;
        TimeStamp = timeStamp;
        PortWithTrace = portWithTrace;
        BaseRefType = baseRefType;
        TraceState = traceState;
    }
}