using Fibertest.Dto;

namespace Fibertest.Graph
{
    
    public class Measurement
    {
        public DateTime MeasurementTimestamp;
        public DateTime EventRegistrationTimestamp;
        public Guid RtuId;
        public Guid TraceId;
        public BaseRefType BaseRefType;
        public FiberState TraceState;

        public EventStatus EventStatus;
        public DateTime StatusChangedTimestamp;
        public string? StatusChangedByUser;

        public string? Comment;
        public List<AccidentOnTraceV2>? Accidents;

        public int SorFileId;
    }
}
