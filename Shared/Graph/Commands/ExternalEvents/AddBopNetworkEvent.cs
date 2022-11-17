namespace Graph
{
    public class AddBopNetworkEvent
    {
        public DateTime EventTimestamp;
        public string? OtauIp;
        public int TcpPort;
        public string? Serial;
        public Guid RtuId;
        public bool IsOk;
    }
}