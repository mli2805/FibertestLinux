using Fibertest.Dto;

namespace Fibertest.Graph;

public static class ModelTryGetExt
{
    public static bool TryGetRtu(this Model model, Guid rtuId, out Rtu? rtu)
    {
        rtu = model.Rtus.FirstOrDefault(r => r.Id == rtuId);
        return rtu != null;
    }

    public static bool TryGetTrace(this Model model, Guid traceId, out Trace? trace)
    {
        trace = model.Traces.FirstOrDefault(t => t.TraceId == traceId);
        return trace != null;
    }

    public static bool TryGetTraceByOtauPortDto(this Model model, OtauPortDto otauPortDto, out Trace? trace)
    {
        trace = model.Traces.FirstOrDefault(t => t.OtauPort != null
                                                 && t.OtauPort.OtauId == otauPortDto.OtauId
                                                 && t.OtauPort.Serial == otauPortDto.Serial
                                                 && t.OtauPort.OpticalPort == otauPortDto.OpticalPort);
        return trace != null;
    }
}