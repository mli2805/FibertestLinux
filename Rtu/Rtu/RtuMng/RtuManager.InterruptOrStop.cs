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

        private async Task StopMonitoringAndConnectOtdrWithRecovering(string customer)
        {
            _wasMonitoringOn = _config.Value.Monitoring.IsMonitoringOn;
            if (_config.Value.Monitoring.IsMonitoringOn)
            {
                _logger.Debug(Logs.RtuManager, "StopMonitoring");
                await StopMonitoring(customer);
            }

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
            await StopMonitoring("Stop monitoring");
            return new RequestAnswer(ReturnCode.Ok);
        }

        private async Task StopMonitoring(string caller)
        {
            if (!_config.Value.Monitoring.IsMonitoringOn)
            {
                _logger.EmptyAndLog(Logs.RtuManager, $"{caller}: RTU is in MANUAL mode already");
                return;
            }
           
            _config.Update(c => c.Monitoring.IsMonitoringOn = false);
            _rtuManagerCts?.Cancel();

            // if Lmax = 240km and Time = 10min one step lasts 5-6 sec
            // important - OTDR is busy until current measurement really stops
            await Task.Delay(TimeSpan.FromSeconds(6));

            _logger.EmptyAndLog(Logs.RtuManager, $"{caller}: Interrupting current measurement...");
            SendCurrentMonitoringStep(MonitoringCurrentStep.Interrupted);
        }

    }
}
