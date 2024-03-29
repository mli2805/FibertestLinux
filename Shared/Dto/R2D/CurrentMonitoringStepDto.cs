﻿namespace Fibertest.Dto;

public class CurrentMonitoringStepDto : BaseRequest
{
    public Guid RtuId;

    public MonitoringCurrentStep Step;
    public PortWithTraceDto? PortWithTraceDto = null!;
    public BaseRefType BaseRefType;
}