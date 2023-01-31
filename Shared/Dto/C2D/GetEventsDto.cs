namespace Fibertest.Dto
{

    public class GetEventsDto : BaseRequest
    {
        public int Revision;

        public override string What => "GetEvents";
    }
    
    public class GetDiskSpaceDto : BaseRequest
    {
        public override string What => "GetDiskSpace";
    }
}