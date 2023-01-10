namespace Fibertest.Dto;

public class InterruptMeasurementDto : BaseRtuRequest
{
    public InterruptMeasurementDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public override string What => "InterruptMeasurement";
    public override RtuOccupation Why => RtuOccupation.None;

}