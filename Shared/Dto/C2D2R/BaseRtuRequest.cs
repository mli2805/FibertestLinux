namespace Fibertest.Dto;

public class BaseRtuRequest
{
    public string ClientConnectionId = "";
    public Guid RtuId;
    public RtuMaker RtuMaker;

    public BaseRtuRequest(Guid rtuId, RtuMaker rtuMaker)
    {
        RtuId = rtuId;
        RtuMaker = rtuMaker;
    }

    public virtual string What => "BaseRtuRequest";
    public virtual RtuOccupation Why => RtuOccupation.Xxx;
}