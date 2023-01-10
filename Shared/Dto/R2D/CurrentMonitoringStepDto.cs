namespace Fibertest.Dto;

public class CurrentMonitoringStepDto
{
    public Guid RtuId;

    public MonitoringCurrentStep Step;
    public PortWithTraceDto? PortWithTraceDto = null!;
    public BaseRefType BaseRefType;
}