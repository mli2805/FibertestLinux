namespace Fibertest.Dto;

public class AssignBaseRefsDto : BaseRtuRequest
{
    // public AssignBaseRefsDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    // {
    //     BaseRefs = new();
    //     DeleteOldSorFileIds = new();
    // }

    public AssignBaseRefsDto(Guid rtuId, RtuMaker rtuMaker, Guid traceId, 
        List<BaseRefDto> baseRefs, List<int> deleteOldSorFileIds) : base(rtuId, rtuMaker)
    {
        TraceId = traceId;
        BaseRefs = baseRefs;
        DeleteOldSorFileIds = deleteOldSorFileIds;
    }

    public string? OtdrId; //  in VeEX RTU main OTDR has its own ID
    public Guid TraceId;

    public OtauPortDto? OtauPortDto; // could be null if trace isn't attached to port yet
    public OtauPortDto? MainOtauPortDto; // optional, filled in if trace attached to the child otau

    public List<BaseRefDto> BaseRefs;
    public List<int> DeleteOldSorFileIds;

    public override string What => "AssignBaseRefs";
    public override RtuOccupation Why => RtuOccupation.AssignBaseRefs;

    public AssignBaseRefsDto ShallowCopy()
    {
        return (AssignBaseRefsDto)MemberwiseClone();
    }
}