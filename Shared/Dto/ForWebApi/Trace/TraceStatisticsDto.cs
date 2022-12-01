namespace Fibertest.Dto;

public class TraceStatisticsDto
{
    public TraceHeaderDto Header = new TraceHeaderDto();
    public List<BaseRefInfoDto>? BaseRefs;
    public int MeasFullCount;
    public List<MeasurementDto>? MeasPortion;
}