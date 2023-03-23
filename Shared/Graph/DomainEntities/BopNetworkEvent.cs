namespace Fibertest.Graph;

[Serializable]
public class BopNetworkEvent
{
    public int Ordinal;

    public DateTime EventTimestamp;
    public string Serial = null!;
    public string OtauIp = null!;
    public int TcpPort;
    public Guid RtuId;
    public bool IsOk;
}