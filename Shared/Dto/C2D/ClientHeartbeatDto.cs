namespace Fibertest.Dto
{
    public class ClientHeartbeatDto : BaseRequest
    {
        public string? ClientIp;

        public override string What => "Heartbeat";

    }
}