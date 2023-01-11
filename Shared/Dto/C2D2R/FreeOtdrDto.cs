namespace Fibertest.Dto;

public class FreeOtdrDto : BaseRtuRequest
{
    public FreeOtdrDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public override string What => "FreeOtdr";
    public override RtuOccupation Why => RtuOccupation.None;
}