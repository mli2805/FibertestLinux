namespace Fibertest.Dto
{
    public class FreeOtdrDto : BaseRtuRequest
    {
        public FreeOtdrDto(string connectionId, Guid rtuId, RtuMaker rtuMaker) : base(connectionId, rtuId, rtuMaker)
        {
        }

        public override string What => "FreeOtdr";
        public override RtuOccupation Why => RtuOccupation.None;
    }
}
