namespace Fibertest.Dto
{
    public class RtuDto
    {
        public Guid RtuId;
        public RtuMaker RtuMaker;
        public string? Title;
        public string? OtdrId; // ditto
        public VeexOtau MainVeexOtau = new VeexOtau(); // in Veex RTU it is a separate unit

        public string? Mfid;
        public string? Mfsn;
        public string? Omid;
        public string? Omsn;
        public string? Serial;

        public int OwnPortCount;
        public int FullPortCount;
        public List<ChildDto> Children = new List<ChildDto>();

        public NetAddress? MainChannel;
        public RtuPartState MainChannelState;
        public NetAddress? ReserveChannel;
        public RtuPartState ReserveChannelState;
        public bool IsReserveChannelSet;
        public NetAddress? OtdrNetAddress;
        public RtuPartState BopState;

        public MonitoringState MonitoringMode;

        public string? Version;
        public string? Version2;
    }
}
