using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public partial class RtuManager
    {
        // user request
        public async Task<RequestAnswer> InterruptMeasurement(InterruptMeasurementDto dto)
        {
            _logger.Info(Logs.RtuManager, $"Client {dto.ClientConnectionId}: Interrupting current measurement...");
            await Task.Delay(0);
            _rtuManagerCts?.Cancel();
            return new RequestAnswer(ReturnCode.Ok);
        }

        // private async Task StopMonitoringAndConnectOtdrWithRecovering(string customer)
        // {
        //     _wasMonitoringOn = _config.Value.Monitoring.IsMonitoringOn;
        //     if (_config.Value.Monitoring.IsMonitoringOn)
        //     {
        //         _logger.Debug(Logs.RtuManager, "StopMonitoring");
        //         await StopMonitoring(customer);
        //     }
        //
        //     _logger.EmptyAndLog(Logs.RtuManager, $"Start {customer}.");
        //
        //     if (!_wasMonitoringOn)
        //     {
        //         var res = _otdrManager.ConnectOtdr();
        //         if (!res)
        //         {
        //             var recovery = await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
        //             if (recovery != ReturnCode.Ok)
        //                 await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
        //         }
        //     }
        // }

        private async Task ConnectOtdrWithRecovering()
        {
            var res = _otdrManager.ConnectOtdr();
            if (!res)
            {
                var recovery = await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                if (recovery != ReturnCode.Ok)
                    await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            }
        }

        // user request
        public async Task<RequestAnswer> StopMonitoring()
        {
            await StopMonitoring("Stop monitoring");
            _otdrManager.DisconnectOtdr();
            return new RequestAnswer(ReturnCode.Ok);
        }

        private async Task StopMonitoring(string caller)
        {
            if (!_config.Value.Monitoring.IsMonitoringOn)
            {
                _logger.EmptyAndLog(Logs.RtuManager, $"{caller}: RTU is in MANUAL mode already");
                return;
            }

            _logger.EmptyAndLog(Logs.RtuManager, $"{caller}: Interrupt measurement requested...");
            _wasMonitoringOn = true;
            _config.Update(c => c.Monitoring.IsMonitoringOn = false);
            _rtuManagerCts?.Cancel();

            // if Lmax = 240km and Time = 10min one step lasts 5-6 sec
            // important - OTDR is busy until current measurement really stops
            await Task.Delay(TimeSpan.FromSeconds(6));

            _logger.EmptyAndLog(Logs.RtuManager, "Monitoring stopped");
            SendCurrentMonitoringStep(MonitoringCurrentStep.Interrupted);
        }

    }
}
