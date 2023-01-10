namespace Fibertest.Dto;

public class PrepareReflectMeasurementDto : BaseRtuRequest
{
    public PrepareReflectMeasurementDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public string OtdrId = string.Empty; //  in VeEX RTU main OTDR has its own ID
    public OtauPortDto? OtauPortDto; // could be null if trace isn't attached to port yet
    public OtauPortDto? MainOtauPortDto; // optional, filled in if trace attached to the child otau

    public override string What => "PrepareReflectMeasurement";
    public override RtuOccupation Why => RtuOccupation.None;
}