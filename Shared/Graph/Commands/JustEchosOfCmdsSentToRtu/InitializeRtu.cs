using Fibertest.Dto;

namespace Fibertest.Graph
{
    public class InitializeRtu
    {
        public Guid Id;
        public RtuMaker Maker;

        // public string? OtauId; // in VeEX RTU main OTAU has its own ID
        public string? OtdrId; // ditto
        public VeexOtau MainVeexOtau = new(); // in Veex RTU it is a separate unit

        public string? Mfid;
        public string? Mfsn;
        public string? Omid;
        public string? Omsn;

        public NetAddress? MainChannel;
        public RtuPartState MainChannelState;
        public bool IsReserveChannelSet = false;
        public NetAddress? ReserveChannel;
        public RtuPartState ReserveChannelState;
        public NetAddress? OtauNetAddress; // IP the same as Otdr, Charon
        public string? Serial;
        public int OwnPortCount;
        public int FullPortCount;
        public string? Version;
        public string? Version2;

        public Dictionary<int, OtauDto>? Children;
        public bool IsMonitoringOn;
        public TreeOfAcceptableMeasParams AcceptableMeasParams = new();
    }
}