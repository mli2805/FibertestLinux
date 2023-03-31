namespace Fibertest.Dto;

public class MonitoringResultDto : BaseRequest
{
    public MeasurementResult MeasurementResult;
    public Guid RtuId;
    public DateTime TimeStamp;
    public PortWithTraceDto PortWithTrace = null!;
    public BaseRefType BaseRefType;
    public FiberState TraceState;
    public byte[]? SorBytes;

    public MonitoringResultDto() {} // json serialization

    public MonitoringResultDto(Guid rtuId, 
        DateTime timeStamp, PortWithTraceDto portWithTrace, BaseRefType baseRefType, FiberState traceState)
    {
        RtuId = rtuId;
        TimeStamp = timeStamp;
        PortWithTrace = portWithTrace;
        BaseRefType = baseRefType;
        TraceState = traceState;
    } 
    
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

    public override string What => "MonitoringResult";

}