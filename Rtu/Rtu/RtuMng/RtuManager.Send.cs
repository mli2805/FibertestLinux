using Fibertest.Dto;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    private readonly object _isSenderBusyLocker = new object();
    private bool _isSenderBusy;

    private bool IsSenderBusy
    {
        get
        {
            lock (_isSenderBusyLocker)
            {
                return _isSenderBusy;
            }
        }
        set
        {
            lock (_isSenderBusyLocker)
            {
                _isSenderBusy = value;
            }
        }
    }

    public async void SendCurrentMonitoringStep(MonitoringCurrentStep currentStep,
        MonitoringPort? monitoringPort = null, BaseRefType baseRefType = BaseRefType.None)
    {
        if (IsSenderBusy)
            return;

        IsSenderBusy = true;

        var dto = CreateStepDto(currentStep, monitoringPort, baseRefType);

        await _grpcR2DService.SendAnyR2DRequest<CurrentMonitoringStepDto, RequestAnswer>(dto);

        IsSenderBusy = false;
    }

    private CurrentMonitoringStepDto CreateStepDto(MonitoringCurrentStep currentStep,
        MonitoringPort? monitoringPort = null, BaseRefType baseRefType = BaseRefType.None)
    {
        return new CurrentMonitoringStepDto()
        {
            RtuId = _id,
            Step = currentStep,
            PortWithTraceDto = monitoringPort == null
                ? null
                : new PortWithTraceDto(
                    new OtauPortDto(monitoringPort.OpticalPort, monitoringPort.IsPortOnMainCharon)
                    {
                        Serial = monitoringPort.CharonSerial
                    },
                    monitoringPort.TraceId),
            BaseRefType = baseRefType,
        };
    }
}