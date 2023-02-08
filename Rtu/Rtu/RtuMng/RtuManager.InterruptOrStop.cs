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
            _logger.LogInfo(Logs.RtuManager, $"_wasMonitoringOn is {_wasMonitoringOn}.");

            if (!_wasMonitoringOn)
            {
                var res = await _otdrManager.ConnectOtdr();
                if (!res)
                // var res = await _otdrManager.InitializeOtdr();
                // if (res.ReturnCode != ReturnCode.Ok)
                {
                    var recovery = await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                    // res = _otdrManager.ConnectOtdr(_mainCharon.NetAddress.Ip4Address);
                    // if (!res)
                    if (recovery != ReturnCode.Ok)
                        await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                }
            }
        }

        public async Task<RequestAnswer> StopMonitoring()
        {
            await Task.Delay(1);
            StopMonitoring("Stop monitoring");
            return new RequestAnswer(ReturnCode.Ok);
        }

        private void StopMonitoring(string caller)
        {
            if (!_config.Value.Monitoring.IsMonitoringOn)
            {
                _logger.LogInfo(Logs.RtuManager, $"{caller}: RTU is in MANUAL mode already");
                return;
            }

            _config.Update(c => c.Monitoring.IsMonitoringOn = false);
            _logger.LogInfo(Logs.RtuManager, $"{caller}: Interrupting current measurement...");
            _cancellationTokenSource?.Cancel();

            // if Lmax = 240km and Time = 10min one step lasts 5-6 sec
            Thread.Sleep(TimeSpan.FromSeconds(6));
        }

    }
}
