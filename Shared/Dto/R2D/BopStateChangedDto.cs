namespace Fibertest.Dto;

public class BopStateChangedDto : BaseRequest
{
    public Guid RtuId;
    public string Serial = null!;
    public string OtauIp = null!;
    public int TcpPort;
    public bool IsOk;
 
    public override string What => "BopStateChanged";

}