﻿namespace Graph
{
    public class BopNetworkEventAdded
    {
        public int Ordinal;

        public DateTime EventTimestamp;
        public string? Serial;
        public string? OtauIp;
        public int TcpPort;
        public Guid RtuId;
        public bool IsOk;
    }
}