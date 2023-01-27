namespace Fibertest.Dto
{
    
    public class DetachTraceDto : BaseRequest
    {
        public Guid TraceId;
        public override string What => "DetachTrace";

    }
}