using Fibertest.Dto;

namespace Graph
{
    
    public class Trace
    {
        public Guid TraceId;
        public string Title = String.Empty;
        public Guid RtuId; // it's better to store than search through the RTU list

        public FiberState State = FiberState.NotJoined;
        public OtauPortDto? OtauPort; // it's better to store due attach to port, cos search is too complicated
        public bool IsAttached => OtauPort != null;
        public int Port = -1;

        public TraceMode Mode = TraceMode.Light;
        public List<Guid> NodeIds = new();
        public List<Guid> EquipmentIds = new();
        public List<Guid> FiberIds = new();

        public Guid PreciseId = Guid.Empty;
        public TimeSpan PreciseDuration;
        public Guid FastId = Guid.Empty;
        public TimeSpan FastDuration;
        public Guid AdditionalId = Guid.Empty;
        public TimeSpan AdditionalDuration;

        public TraceToTceLinkState TraceToTceLinkState;
        public string? Comment;

        public bool HasAnyBaseRef => PreciseId != Guid.Empty || FastId != Guid.Empty || AdditionalId != Guid.Empty;
        public bool HasEnoughBaseRefsToPerformMonitoring => PreciseId != Guid.Empty && FastId != Guid.Empty;
        public bool IsIncludedInMonitoringCycle;

        public List<Guid> ZoneIds = new();

        public override string ToString()
        {
            return Title;
        }
    }
}
