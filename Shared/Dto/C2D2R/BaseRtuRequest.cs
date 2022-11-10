namespace Fibertest.Dto;

public class BaseRtuRequest
{
    public string ConnectionId;
    public Guid RtuId;
    public RtuMaker RtuMaker;

    public BaseRtuRequest(string connectionId, Guid rtuId, RtuMaker rtuMaker)
    {
        ConnectionId = connectionId;
        RtuId = rtuId;
        RtuMaker = rtuMaker;
    }

    public virtual string What => "BaseRtuRequest";
    public virtual RtuOccupation Why => RtuOccupation.Xxx;
}