using Fibertest.Dto;

// ReSharper disable InconsistentNaming

namespace Graph
{
    
    public class NetworkEvent
    {
        public int Ordinal;

        public DateTime EventTimestamp;
        public Guid RtuId;
        public ChannelEvent OnMainChannel;
        public ChannelEvent OnReserveChannel;

        public bool IsRtuAvailable;

    }
}