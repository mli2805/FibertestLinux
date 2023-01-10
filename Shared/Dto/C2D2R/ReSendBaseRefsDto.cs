namespace Fibertest.Dto;

public class ReSendBaseRefsDto : BaseRtuRequest
{
    public ReSendBaseRefsDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public string OtdrId = string.Empty; //  in VeEX RTU main OTDR has its own ID

    public Guid TraceId;
    public OtauPortDto? OtauPortDto; // could be null if trace isn't attached to port yet
    public OtauPortDto? MainOtauPortDto; // optional, filled in if trace attached to the child otau

    public List<BaseRefDto> BaseRefDtos = new List<BaseRefDto>();

    public override string What => "ReSendBaseRefs";
    public override RtuOccupation Why => RtuOccupation.AssignBaseRefs;
}