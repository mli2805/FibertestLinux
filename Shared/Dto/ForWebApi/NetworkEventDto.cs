namespace Fibertest.Dto
{
    public class NetworkEventDto
    {
        public int EventId;
        public DateTime EventRegistrationTimestamp;
        public Guid RtuId;
        public string? RtuTitle;

        public bool IsRtuAvailable;
        public ChannelEvent OnMainChannel;
        public ChannelEvent OnReserveChannel;
    }

    public class NetworkEventsRequestedDto
    {
        public int FullCount;
        public List<NetworkEventDto>? EventPortion;
    }
}
