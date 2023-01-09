namespace Fibertest.Dto
{

    public class DoOutOfTurnPreciseMeasurementDto : BaseRtuRequest
    {
        public DoOutOfTurnPreciseMeasurementDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
        {
        }

        public Guid Id; // Request ID ?
        public PortWithTraceDto? PortWithTraceDto;

        public bool IsTrapCaused; // false means user's measurement

        public override string What => "DoOutOfTurnPreciseMeasurement";
        public override RtuOccupation Why => RtuOccupation.DoPreciseMeasurementOutOfTurn;

    }
}