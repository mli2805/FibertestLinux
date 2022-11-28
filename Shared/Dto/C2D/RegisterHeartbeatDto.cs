namespace Fibertest.Dto;

public class RegisterHeartbeatDto : BaseRequest
{
    public RegisterHeartbeatDto(string connectionId) : base(connectionId)
    {
    }

    public override string What => "RegisterHeartbeat";

}