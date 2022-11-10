namespace Fibertest.Dto;

public class InitializeRtuDto : BaseRtuRequest
{
    public InitializeRtuDto(string connectionId, Guid rtuId, RtuMaker rtuMaker) : base(connectionId, rtuId, rtuMaker)
    {
    }

    public VeexOtau MainVeexOtau = new VeexOtau(); // in Veex RTU it is a separate unit
    public DoubleAddress? ServerAddresses;
    public DoubleAddress? RtuAddresses;

    public bool IsFirstInitialization;

    // RTU properties after previous initialization
    public string? Serial;
    public int OwnPortCount;
    public Dictionary<int, OtauDto>? Children;

    public override string What => "InitializeRtu";
    public override RtuOccupation Why => RtuOccupation.InitializeRtu;
}