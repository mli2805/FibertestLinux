using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public partial class RtuManager
    {
        public void InterruptMeasurement(InterruptMeasurementDto dto)
        {
            _logger.Info(Logs.RtuManager, $"Client {dto.ClientConnectionId}: Interrupting current measurement...");
            _rtuManagerCts?.Cancel();
        }

        private async void StopMonitoringAndConnectOtdrWithRecovering(string customer)
        {
            _wasMonitoringOn = IsMonitoringOn;
            if (IsMonitoringOn)
                StopMonitoring(customer);

            _logger.EmptyAndLog(Logs.RtuManager, $"Start {customer}.");

            if (!_wasMonitoringOn)
            {
                var res = _otdrManager.ConnectOtdr();
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
            await Task.Delay(0);
            StopMonitoring("Stop monitoring");
            return new RequestAnswer(ReturnCode.Ok);
        }

        private void StopMonitoring(string caller)
        {
            if (!_config.Value.Monitoring.IsMonitoringOn)
            {
                _logger.EmptyAndLog(Logs.RtuManager, $"{caller}: RTU is in MANUAL mode already");
                return;
            }
           
            _config.Update(c => c.Monitoring.IsMonitoringOn = false);
            _rtuManagerCts?.Cancel();

            // if Lmax = 240km and Time = 10min one step lasts 5-6 sec
            // Thread.Sleep(TimeSpan.FromSeconds(6));

            _logger.EmptyAndLog(Logs.RtuManager, $"{caller}: Interrupting current measurement...");
            SendCurrentMonitoringStep(MonitoringCurrentStep.Interrupted);
        }

    }
}
