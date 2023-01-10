namespace Fibertest.Dto;

public class AssignBaseRefsDto : BaseRtuRequest
{
    public AssignBaseRefsDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public string OtdrId = string.Empty; //  in VeEX RTU main OTDR has its own ID
    public Guid TraceId;
    public bool IsAutoBase;

    public OtauPortDto? OtauPortDto; // could be null if trace isn't attached to port yet
    public OtauPortDto? MainOtauPortDto; // optional, filled in if trace attached to the child otau

    public List<BaseRefDto> BaseRefs = new List<BaseRefDto>();
    public List<int> DeleteOldSorFileIds = new List<int>();

    public override string What => "AssignBaseRefs";
    public override RtuOccupation Why => RtuOccupation.AssignBaseRefs;
}