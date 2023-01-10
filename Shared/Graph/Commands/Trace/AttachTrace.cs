using Fibertest.Dto;

namespace Fibertest.Graph;

public class AttachTrace
{
    public Guid TraceId;
    public OtauPortDto OtauPortDto;

    public AttachTrace(Guid traceId, OtauPortDto otauPortDto)
    {
        TraceId = traceId;
        OtauPortDto = otauPortDto;
    }
}