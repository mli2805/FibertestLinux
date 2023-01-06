namespace Fibertest.Rtu
{
    public class DamagedOtau
    {
        public string Ip;
        public int TcpPort;
        public string Serial;
        public DateTime RebootStarted;
        public int RebootAttempts;

        public DamagedOtau(string ip, int tcpPort, string serial)
        {
            Ip = ip;
            TcpPort = tcpPort;
            Serial = serial;
            RebootStarted = DateTime.Now;
            RebootAttempts = 0;
        }
    }
}
