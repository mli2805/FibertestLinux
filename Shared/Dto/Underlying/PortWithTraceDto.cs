namespace Fibertest.Dto;

public class PortWithTraceDto
{
    public OtauPortDto OtauPort = new();
    public Guid TraceId;
    public FiberState LastTraceState;
}