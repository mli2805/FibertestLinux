using Fibertest.Dto;

namespace Fibertest.Graph;

public class Rtu
{
    public Guid Id;

    // public string? OtauId; // in VeEX RTU main OTAU has its own ID
    public string? OtdrId; // ditto
    public VeexOtau MainVeexOtau = new(); // in Veex RTU it is a separate unit

    public RtuMaker RtuMaker;
    public string? Mfid;
    public string? Mfsn;
    public string? Omid;
    public string? Omsn;

    public Guid NodeId;
    public string? Title;
    public string? Comment;

    public NetAddress MainChannel = new(@"", -1);
    public RtuPartState MainChannelState;
    public NetAddress ReserveChannel = new("", -1);
    public RtuPartState ReserveChannelState;
    public bool IsReserveChannelSet = false;
    public NetAddress OtdrNetAddress = new(@"0.0.0.0", 1500); // real address
    public bool IsAvailable => MainChannelState == RtuPartState.Ok ||
                               ReserveChannelState == RtuPartState.Ok;
    public bool IsAllRight => MainChannelState == RtuPartState.Ok &&
                              ReserveChannelState != RtuPartState.Broken;

    // pair OTAU ID - is OK or not
    private Dictionary<Guid, bool> _otauStates = new();
    public void SetOtauState(Guid otauId, bool isOk)
    {
        if (_otauStates.ContainsKey(otauId))
            _otauStates[otauId] = isOk;
        else
            _otauStates.Add(otauId, isOk);
    }

    public void RemoveOtauState(Guid otauId)
    {
        if (_otauStates.ContainsKey(otauId))
            _otauStates.Remove(otauId);
    }

    public RtuPartState BopState
    {
        get
        {
            return _otauStates.Count == 0
                ? RtuPartState.NotSetYet
                : _otauStates.Any(s => s.Value != true)
                    ? RtuPartState.Broken
                    : RtuPartState.Ok;
        }
    }
        
    public string? Serial;
    public int OwnPortCount;
    public bool IsInitialized => OwnPortCount != 0;
    public int FullPortCount;

    public string PortCount => OwnPortCount == FullPortCount ? $@"{FullPortCount}" : $@"{OwnPortCount} / {FullPortCount}";

    public string? Version;
    public string? Version2;

    public Dictionary<int, OtauDto> Children = new();

    public MonitoringState MonitoringState;
    public Frequency PreciseMeas = Frequency.EveryHour;
    public Frequency PreciseSave = Frequency.DoNot;
    public Frequency FastSave = Frequency.DoNot;

    public TreeOfAcceptableMeasParams AcceptableMeasParams = new();

    public List<Guid> ZoneIds = new();

}