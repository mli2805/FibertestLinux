namespace Fibertest.Dto;

public class RtuInitializedDto : RequestAnswer
{
    public Guid RtuId;
    public RtuMaker Maker;

    public VeexOtau MainVeexOtau = new VeexOtau(); // in Veex RTU it is a separate unit
    public string? OtdrId; // ditto

    public string? Mfid;
    public string? Mfsn;
    public string? Omid;
    public string? Omsn;

    public bool IsInitialized => ReturnCode == ReturnCode.RtuInitializedSuccessfully;
        
    public DoubleAddress? RtuAddresses;
    public NetAddress? OtdrAddress;

    public string? Serial;
    public int OwnPortCount;
    public int FullPortCount;
    public string? Version;
    public string? Version2;
    public string? VersionIitOtdr;
        
    public Dictionary<int, OtauDto>? Children;
        
    public bool IsMonitoringOn;
        
    public TreeOfAcceptableMeasParams? AcceptableMeasParams;

    public RtuInitializedDto(ReturnCode returnCode) : base(returnCode)
    {
    }
}