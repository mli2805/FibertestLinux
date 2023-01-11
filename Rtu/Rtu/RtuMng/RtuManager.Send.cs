using Fibertest.Dto;
using Newtonsoft.Json;

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

        var dto = new CurrentMonitoringStepDto()
        {
            RtuId = _id,
            Step = currentStep,
            PortWithTraceDto = monitoringPort == null
                ? null
                : new PortWithTraceDto(
                    new OtauPortDto(monitoringPort.OpticalPort, monitoringPort.IsPortOnMainCharon, monitoringPort.CharonSerial),
                    monitoringPort.TraceId),
            BaseRefType = baseRefType,
        };

        await _grpcSender.SendToDc(JsonConvert.SerializeObject(dto, JsonSerializerSettings));

        IsSenderBusy = false;
    }
}