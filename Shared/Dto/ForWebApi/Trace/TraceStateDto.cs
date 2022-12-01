namespace Fibertest.Dto
{
    public class TraceStateDto
    {
        public TraceHeaderDto Header = new TraceHeaderDto();
        public Guid RtuId;
        public Guid TraceId;
        public FiberState TraceState;
        public BaseRefType BaseRefType;
        public EventStatus EventStatus;
        public string? Comment;
        public DateTime MeasurementTimestamp;
        public DateTime RegistrationTimestamp;
        public int SorFileId;
        public List<AccidentLineDto> Accidents = new List<AccidentLineDto>();
        public bool IsLastStateForThisTrace;
        public bool IsLastAccidentForThisTrace;
    }
}