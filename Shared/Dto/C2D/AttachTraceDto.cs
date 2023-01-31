namespace Fibertest.Dto
{
    
    public class AttachTraceDto : BaseRequest
    {
        public RtuMaker RtuMaker;
        public Guid TraceId;
        public OtauPortDto? OtauPortDto; // if trace attached to main otau use only this property
        public OtauPortDto? MainOtauPortDto; // veex cannot measure bop without this, use it if trace attached to bop

        public override string What => "AttachTrace";
    }
}