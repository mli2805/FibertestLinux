namespace Fibertest.Dto
{

    public class GetEventsDto : BaseRequest
    {
        public int Revision;

        public override string What => "GetEvents";
    }
}