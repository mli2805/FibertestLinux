namespace Fibertest.Dto
{
    public class FreeOtdrDto : RtuRequestHeader
    {
        public FreeOtdrDto(string connectionId, Guid rtuId, RtuMaker rtuMaker) : base(connectionId, rtuId, rtuMaker)
        {
        }
    }
}
