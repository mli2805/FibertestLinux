namespace Fibertest.Dto
{
    public class RtuStateDto
    {
        public string? RtuId;
        public string? RtuTitle;

        public string? MainChannel;
        public RtuPartState MainChannelState;
        public string? ReserveChannel;
        public RtuPartState ReserveChannelState;
        public bool IsReserveChannelSet;

        public RtuPartState BopState;
        public MonitoringState MonitoringMode;
        public FiberState TracesState;

        public int OwnPortCount;
        public int FullPortCount;
        public int BopCount;
        public int TraceCount;

        public List<RtuStateChildDto> Children = new List<RtuStateChildDto>();
    }
}