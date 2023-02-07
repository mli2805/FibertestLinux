using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public partial class RtuManager
    {
        public void InterruptMeasurement(InterruptMeasurementDto dto)
        {
            _logger.LogInfo(Logs.RtuManager, $"Client {dto.ClientConnectionId}: Interrupting current measurement...");
            _cancellationTokenSource?.Cancel();
        }

        private async void StopMonitoringAndConnectOtdrWithRecovering(string customer)
        {
            _wasMonitoringOn = IsMonitoringOn;
            if (IsMonitoringOn)
                StopMonitoring(customer);

            _logger.EmptyAndLog(Logs.RtuManager, $"Start {customer}.");

            if (!_wasMonitoringOn)
            {
                var res = await _otdrManager.ConnectOtdr();
                if (!res)
                {
                    var recovery = await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                    // res = _otdrManager.ConnectOtdr(_mainCharon.NetAddress.Ip4Address);
                    // if (!res)
                    if (recovery != ReturnCode.Ok)
                        await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                }
            }
        }
    }
}
