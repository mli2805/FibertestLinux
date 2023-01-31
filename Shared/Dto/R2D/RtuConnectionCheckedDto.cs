namespace Fibertest.Dto
{
    public class RtuConnectionCheckedDto : RequestAnswer
    {
        public Guid RtuId;
        public bool IsConnectionSuccessful;
        public NetAddress? NetAddress;
        public bool IsPingSuccessful; // check if rtu service connection failed

        public RtuConnectionCheckedDto() {}
        public RtuConnectionCheckedDto(ReturnCode returnCode) : base(returnCode)
        {
        }
    }
}