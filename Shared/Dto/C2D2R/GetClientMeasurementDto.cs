namespace Fibertest.Dto
{
    
    public class GetClientMeasurementDto : BaseRtuRequest
    {
        public string VeexMeasurementId = string.Empty;

        public GetClientMeasurementDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
        {
        }

        public override string What => "GetClientMeasurement";
        public override RtuOccupation Why => RtuOccupation.None;

    }
}