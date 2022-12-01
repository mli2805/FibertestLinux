namespace Fibertest.Dto
{
    public class OpticalEventDto
    {
        public int EventId;
        public DateTime MeasurementTimestamp;
        public DateTime EventRegistrationTimestamp;
        public string? RtuTitle;
        public Guid RtuId;

        public Guid TraceId;
        public string? TraceTitle;

        public BaseRefType BaseRefType;
        public FiberState TraceState;

        public EventStatus EventStatus;
        public DateTime StatusChangedTimestamp;
        public string? StatusChangedByUser;

        public string? Comment;
    }

    public class OpticalEventsRequestedDto
    {
        public int FullCount;
        public List<OpticalEventDto>? EventPortion;
    }

}