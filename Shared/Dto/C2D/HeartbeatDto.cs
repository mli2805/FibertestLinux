namespace Fibertest.Dto
{
    public class HeartbeatDto : BaseRequest
    {
        public string? ClientIp;

        public override string What => "Heartbeat";

    }
}