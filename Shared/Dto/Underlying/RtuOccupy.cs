namespace Fibertest.Dto;

public enum RtuOccupation
{
    None, Xxx,
    RemoveRtu, CleanOrRemoveTrace, AttachOrDetachOtau, AttachTrace,
    DoAutoBaseMeasurement, DoMeasurementClient, DoPreciseMeasurementOutOfTurn,  
    InitializeRtu, ApplyMonitoringSettings, DetachTraces, AssignBaseRefs,
}

public class RtuOccupationState
{
    private const int TimeoutSec = 100;

    public RtuOccupation RtuOccupation;
    public string? UserName; // who started occupation
    public DateTime Expired;

    public RtuOccupationState()
    {
        RtuOccupation = RtuOccupation.None;
    }

    public RtuOccupationState(RtuOccupation rtuOccupation, string? userName)
    {
        RtuOccupation = rtuOccupation;
        UserName = userName;
        Expired = DateTime.Now.AddSeconds(TimeoutSec);
    }
}

public class OccupyRtuDto
{
    public string? ConnectionId;
    public Guid RtuId;
    public RtuOccupationState State;

    public OccupyRtuDto(string connectionId, Guid rtuId, RtuOccupationState state)
    {
        ConnectionId = connectionId;
        RtuId = rtuId;
        State = state;
    }
}