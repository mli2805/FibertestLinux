namespace Fibertest.Dto
{
    public class TraceDto : ChildDto
    {
        public Guid TraceId;
        public Guid RtuId;
        public string? Title;
        public OtauPortDto? OtauPort;
        public bool IsAttached;

        public FiberState State;

        public bool HasEnoughBaseRefsToPerformMonitoring;
        public bool IsIncludedInMonitoringCycle;

        public TraceDto(ChildType childType) : base(childType)
        {
        }
    }
}