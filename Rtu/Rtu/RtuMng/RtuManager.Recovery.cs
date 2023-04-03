using Fibertest.Dto;
using Fibertest.Utils;
using Fibertest.Utils.Recovery;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    private TimeSpan _mikrotikRebootTimeout;

    private async Task<ReturnCode> RunMainCharonRecovery()
    {
        _mikrotikRebootTimeout = TimeSpan.FromSeconds(_config.Value.Recovery.MikrotikRebootTimeout);
        var previousStep = _config.Value.Recovery.RecoveryStep;

        switch (previousStep)
        {
            case RecoveryStep.Ok:
                _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.ResetArpAndCharon);
                RestoreFunctions.ClearArp(_logger);
                var recoveryResult = await InitializeRtu(null, true);
                if (recoveryResult.IsInitialized)
                    _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.Ok);
                return recoveryResult.ReturnCode; // Reset Charon
            case RecoveryStep.ResetArpAndCharon:
                _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.RestartService);
                _logger.Info(Logs.RtuManager, "Recovery procedure: Exit rtu service.");
                _logger.Info(Logs.RtuService, "Recovery procedure: Exit rtu service.");
                Environment.FailFast("Recovery procedure: Exit rtu service.");
                // ReSharper disable once HeuristicUnreachableCode
                return ReturnCode.Ok;
            case RecoveryStep.RestartService:
                var enabled = _config.Value.Recovery.RebootSystemEnabled;
                if (enabled)
                {
                    _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.RebootPc);
                    var delay = _config.Value.Recovery.RebootSystemDelay;
                    _logger.Info(Logs.RtuManager, "Recovery procedure: Reboot system.");
                    _logger.Info(Logs.RtuService, "Recovery procedure: Reboot system.");
                    RestoreFunctions.RebootSystem(_logger, delay);
                    Thread.Sleep(TimeSpan.FromSeconds(delay + 5));
                    return ReturnCode.Ok;
                }
                else
                {
                    _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.ResetArpAndCharon);
                    RestoreFunctions.ClearArp(_logger);
                    var recoveryResult1 = await InitializeRtu(null, true);
                    if (recoveryResult1.IsInitialized)
                        _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.Ok);
                    return recoveryResult1.ReturnCode;
                }
            case RecoveryStep.RebootPc:
                _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.ResetArpAndCharon);
                RestoreFunctions.ClearArp(_logger);
                var recoveryResult2 = await InitializeRtu(null, true);
                if (recoveryResult2.IsInitialized)
                    _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.Ok);
                return recoveryResult2.ReturnCode;
        }

        return ReturnCode.Ok;
    }

    private void RunAdditionalOtauRecovery(DamagedOtau damagedOtau)
    {
        damagedOtau.RebootStarted = DateTime.Now;
        damagedOtau.RebootAttempts++;

        var mikrotikRebootAttemptsBeforeNotification =
            _config.Value.Recovery.MikrotikRebootAttemptsBeforeNotification;
        if (damagedOtau.RebootAttempts == mikrotikRebootAttemptsBeforeNotification)
            MessageQueue.Send(new BopStateChangedDto()
            {
                RtuId = _id,
                OtauIp = damagedOtau.Ip,
                TcpPort = damagedOtau.TcpPort,
                Serial = damagedOtau.Serial,
                IsOk = false
            });

        _logger.Info(Logs.RtuService, $"Mikrotik {damagedOtau.Ip} reboot N{damagedOtau.RebootAttempts}");
        _logger.Info(Logs.RtuManager, $"Reboot attempt N{damagedOtau.RebootAttempts}");
        var connectionTimeout = _config.Value.Charon.ConnectionTimeout;
        try
        {
            MikrotikInBop.ConnectAndReboot(_logger, Logs.RtuManager.ToInt(), damagedOtau.Ip, connectionTimeout);
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"Cannot connect Mikrotik {damagedOtau.Ip}" + e.Message);
        }

    }

}