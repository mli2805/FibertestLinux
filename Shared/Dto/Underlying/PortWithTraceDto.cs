namespace Fibertest.Dto
{
    public class PortWithTraceDto
    {
        public OtauPortDto? OtauPort;
        public Guid TraceId;
        public FiberState LastTraceState;
    }
}
