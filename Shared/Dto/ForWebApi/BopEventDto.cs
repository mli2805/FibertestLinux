namespace Fibertest.Dto
{
    public class BopEventDto
    {
        public int EventId;
        public DateTime EventRegistrationTimestamp;
        public string? BopAddress;
        public Guid RtuId;
        public string? RtuTitle;
        public string? Serial;

        public bool BopState;
    }

    public class BopEventsRequestedDto
    {
        public int FullCount;
        public List<BopEventDto>? EventPortion;
    }
}
