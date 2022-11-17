using Fibertest.Dto;

namespace Graph
{
    
    public class TraceAttached
    {
        public Guid TraceId;
        public OtauPortDto? OtauPortDto;

        public FiberState PreviousTraceState;
        public List<AccidentOnTraceV2>? AccidentsInLastMeasurement;
    }
}
