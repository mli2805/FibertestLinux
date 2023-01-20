namespace Fibertest.Dto
{

    public class GetEventsDto : BaseRequest
    {
        public string? ClientIp;
        public int Revision;

        public override string What => "GetEvents";
    }
}