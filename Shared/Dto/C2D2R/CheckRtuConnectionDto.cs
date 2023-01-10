namespace Fibertest.Dto;

public class CheckRtuConnectionDto : BaseRtuRequest
{
    public CheckRtuConnectionDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public NetAddress NetAddress = new NetAddress();

    public override string What => "CheckRtuConnection";
    public override RtuOccupation Why => RtuOccupation.None;

}