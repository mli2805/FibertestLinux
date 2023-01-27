namespace Fibertest.Dto
{
    public class RtuConnectionCheckedDto : RequestAnswer
    {
        public string ClientIp;
        public Guid RtuId;
        public bool IsConnectionSuccessful;
        public NetAddress? NetAddress;
        public bool IsPingSuccessful; // check if rtu service connection failed
    }
}