namespace Fibertest.Dto;

public class RtuMonitoringPortDto
{
    public string? Port;
    public string? TraceId;
    public OtauPortDto? OtauPortDto;
    public string? TraceTitle;
    public PortMonitoringMode PortMonitoringMode;
    public int DurationOfPreciseBase;
    public int DurationOfFastBase;
}