using Fibertest.Dto;

// ReSharper disable InconsistentNaming

namespace Fibertest.Graph;

public class NetworkEvent
{
    public int Ordinal;

    public DateTime EventTimestamp;
    public Guid RtuId;
    public ChannelEvent OnMainChannel;
    public ChannelEvent OnReserveChannel;

    public bool IsRtuAvailable;


    public override string ToString()
    {
        var res = $"RTU {RtuId.First6()}";
        if (OnMainChannel != ChannelEvent.Nothing)
            res += $"   Main - {OnMainChannel}";
        if (OnReserveChannel != ChannelEvent.Nothing)
            res += $"   Reserve - {OnReserveChannel}";
        return res;
    }
}