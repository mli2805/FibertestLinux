namespace Fibertest.Dto
{
    
    public class AttachTraceDto : BaseRtuRequest
    {
        public Guid TraceId;
        public OtauPortDto? OtauPortDto; // if trace attached to main otau use only this property
        public OtauPortDto? MainOtauPortDto; // veex cannot measure bop without this, use it if trace attached to bop

        public override string What => "AttachTraceAndSendBaseRefs";

        public AttachTraceDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
        {
        }
    }
}