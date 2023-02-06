namespace Fibertest.Graph;

[Serializable]
public class BopNetworkEvent
{
    public int Ordinal;

    public DateTime EventTimestamp;
    public string? Serial;
    public string? OtauIp;
    public int TcpPort;
    public Guid RtuId;
    public bool IsOk;
}