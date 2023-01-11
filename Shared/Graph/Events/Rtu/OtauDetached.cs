namespace Fibertest.Graph;

public class OtauDetached
{
    public Guid Id; // OtauId
    public Guid RtuId;
    public string? OtauIp;
    public int TcpPort;
    public List<Guid> TracesOnOtau = new List<Guid>();
}