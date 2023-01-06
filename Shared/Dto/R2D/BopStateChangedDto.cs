namespace Fibertest.Dto;

public class BopStateChangedDto
{
    public Guid RtuId;
    public string Serial = null!;
    public string OtauIp = null!;
    public int TcpPort;
    public bool IsOk;
            
}