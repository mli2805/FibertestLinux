

// ReSharper disable InconsistentNaming

using Fibertest.Dto;

namespace Fibertest.Graph;

public class NetworkEventAdded
{
    public int Ordinal;

    public DateTime EventTimestamp;
    public Guid RtuId;
    public ChannelEvent OnMainChannel;
    public ChannelEvent OnReserveChannel;
}