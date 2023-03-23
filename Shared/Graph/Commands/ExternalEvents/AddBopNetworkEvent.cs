namespace Fibertest.Graph;

public class AddBopNetworkEvent
{
    public DateTime EventTimestamp;
    public string OtauIp = null!;
    public int TcpPort;
    public string Serial = null!;
    public Guid RtuId;
    public bool IsOk;
}