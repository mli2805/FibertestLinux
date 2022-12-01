namespace Fibertest.Dto;

public class BaseRequest
{
    public string ConnectionId;

    public BaseRequest(string connectionId)
    {
        ConnectionId = connectionId;
    }
    public virtual string What => "BaseRequest";

}