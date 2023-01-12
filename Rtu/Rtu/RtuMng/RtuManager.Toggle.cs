using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    private readonly List<DamagedOtau> _damagedOtaus = new List<DamagedOtau>();
    private async Task<bool> ToggleToPort(MonitoringPort monitoringPort)
    {
        var cha = monitoringPort.IsPortOnMainCharon
            ? _mainCharon 
            : _mainCharon.GetBopCharonWithLogging(monitoringPort.CharonSerial);

        if (cha == null) return false;
        // TCP port here is not important
        var damagedOtau = _damagedOtaus.FirstOrDefault(b => b.Ip == cha.NetAddress.Ip4Address);
        if (damagedOtau != null)
        {
            _logger.LogInfo(Logs.RtuManager, $"Port is on damaged BOP {damagedOtau.Ip}");
            if (DateTime.Now - damagedOtau.RebootStarted < _mikrotikRebootTimeout)
            {
                _logger.LogInfo(Logs.RtuManager, $"Mikrotik {cha.NetAddress.Ip4Address} is rebooting, step to the next port");
                return false;
            }
            else
            {
                if (cha.OwnPortCount == 0)
                    await InitializeOtau(new RtuInitializedDto());
            }
        }

        SendCurrentMonitoringStep(MonitoringCurrentStep.Toggle, monitoringPort);

        var toggleResult = _mainCharon.SetExtendedActivePort(monitoringPort.CharonSerial, monitoringPort.OpticalPort);
        switch (toggleResult)
        {
            case CharonOperationResult.Ok:
                {
                    _logger.LogInfo(Logs.RtuManager, "Toggled Ok.");
                    // Here TCP port is important
                    if (damagedOtau != null &&
                        damagedOtau.Ip == cha.NetAddress.Ip4Address &&
                        damagedOtau.TcpPort == cha.NetAddress.Port)
                    {
                        _logger.LogInfo(Logs.RtuManager, $"OTAU {cha.NetAddress.ToStringA()} recovered");
                        if (damagedOtau.RebootAttempts >= _recoveryConfig.Value.MikrotikRebootAttemptsBeforeNotification)
                        {
                            _logger.LogInfo(Logs.RtuManager, "Send notification to server.");
                            var dto = new BopStateChangedDto()
                            {
                                RtuId = _id,
                                Serial = monitoringPort.CharonSerial,
                                OtauIp = cha.NetAddress.Ip4Address,
                                TcpPort = cha.NetAddress.Port,
                                IsOk = true,
                            };
                            SendByMsmq(dto);
                        }
                        _damagedOtaus.Remove(damagedOtau);
                    }

                    return true;
                }
            case CharonOperationResult.MainOtauError:
                {
                    _serialPortManager.ShowOnLedDisplay(LedDisplayCode.ErrorTogglePort);
                    if (await RunMainCharonRecovery() != ReturnCode.Ok)
                        await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                    return false;
                }
            case CharonOperationResult.AdditionalOtauError:
                {
                    if (damagedOtau == null)
                    {
                        damagedOtau = new DamagedOtau(cha.NetAddress.Ip4Address, cha.NetAddress.Port, monitoringPort.CharonSerial);
                        _damagedOtaus.Add(damagedOtau);
                    }
                    RunAdditionalOtauRecovery(damagedOtau);
                    return false;
                }
            default:
                {
                    _logger.LogInfo(Logs.RtuManager, _mainCharon.LastErrorMessage);
                    return false;
                }
        }
    }

}