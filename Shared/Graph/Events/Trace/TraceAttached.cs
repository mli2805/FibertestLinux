using Fibertest.Dto;

namespace Fibertest.Graph;

public class TraceAttached
{
    public Guid TraceId;
    public OtauPortDto OtauPortDto = new OtauPortDto();

    public FiberState PreviousTraceState;
    public List<AccidentOnTraceV2> AccidentsInLastMeasurement = new List<AccidentOnTraceV2>();
}