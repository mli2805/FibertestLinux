using Fibertest.Dto;
using Fibertest.Utils;
using Fibertest.Utils.Recovery;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    private TimeSpan _mikrotikRebootTimeout;

    private async Task<ReturnCode> RunMainCharonRecovery()
    {
        _mikrotikRebootTimeout = TimeSpan.FromSeconds(_recoveryConfig.Value.MikrotikRebootTimeout);
        var previousStep = _recoveryConfig.Value.RecoveryStep;

        switch (previousStep)
        {
            case RecoveryStep.Ok:
                _recoveryConfig.Update(c => c.RecoveryStep = RecoveryStep.ResetArpAndCharon);
                RestoreFunctions.ClearArp(_logger);
                var recoveryResult = await InitializeRtu();
                if (recoveryResult.IsInitialized)
                    _recoveryConfig.Update(c => c.RecoveryStep = RecoveryStep.Ok);
                return recoveryResult.ReturnCode; // Reset Charon
            case RecoveryStep.ResetArpAndCharon:
                _recoveryConfig.Update(c => c.RecoveryStep = RecoveryStep.RestartService);
                _logger.LogInfo(Logs.RtuManager, "Recovery procedure: Exit rtu service.");
                _logger.LogInfo(Logs.RtuService, "Recovery procedure: Exit rtu service.");
                Environment.FailFast("Recovery procedure: Exit rtu service.");
                // ReSharper disable once HeuristicUnreachableCode
                return ReturnCode.Ok;
            case RecoveryStep.RestartService:
                var enabled = _recoveryConfig.Value.RebootSystemEnabled;
                if (enabled)
                {
                    _recoveryConfig.Update(c => c.RecoveryStep = RecoveryStep.RebootPc);
                    var delay = _recoveryConfig.Value.RebootSystemDelay;
                    _logger.LogInfo(Logs.RtuManager, "Recovery procedure: Reboot system.");
                    _logger.LogInfo(Logs.RtuService, "Recovery procedure: Reboot system.");
                    RestoreFunctions.RebootSystem(_logger, delay);
                    Thread.Sleep(TimeSpan.FromSeconds(delay + 5));
                    return ReturnCode.Ok;
                }
                else
                {
                    _recoveryConfig.Update(c => c.RecoveryStep = RecoveryStep.ResetArpAndCharon);
                    RestoreFunctions.ClearArp(_logger);
                    var recoveryResult1 = await InitializeRtu();
                    if (recoveryResult1.IsInitialized)
                        _recoveryConfig.Update(c => c.RecoveryStep = RecoveryStep.Ok);
                    return recoveryResult1.ReturnCode;
                }
            case RecoveryStep.RebootPc:
                _recoveryConfig.Update(c => c.RecoveryStep = RecoveryStep.ResetArpAndCharon);
                RestoreFunctions.ClearArp(_logger);
                var recoveryResult2 = await InitializeRtu();
                if (recoveryResult2.IsInitialized)
                    _recoveryConfig.Update(c => c.RecoveryStep = RecoveryStep.Ok);
                return recoveryResult2.ReturnCode;
        }

        return ReturnCode.Ok;
    }

    private void RunAdditionalOtauRecovery(DamagedOtau damagedOtau)
    {
        damagedOtau.RebootStarted = DateTime.Now;
        damagedOtau.RebootAttempts++;

        var mikrotikRebootAttemptsBeforeNotification =
            _recoveryConfig.Value.MikrotikRebootAttemptsBeforeNotification;
        if (damagedOtau.RebootAttempts == mikrotikRebootAttemptsBeforeNotification)
            MessageQueue.Send(new BopStateChangedDto()
            {
                RtuId = _id,
                OtauIp = damagedOtau.Ip,
                TcpPort = damagedOtau.TcpPort,
                Serial = damagedOtau.Serial,
                IsOk = false
            });

        _logger.LogInfo(Logs.RtuService, $"Mikrotik {damagedOtau.Ip} reboot N{damagedOtau.RebootAttempts}");
        _logger.LogInfo(Logs.RtuManager, $"Reboot attempt N{damagedOtau.RebootAttempts}");
        var connectionTimeout = _fullConfig.Value.Charon.ConnectionTimeout;
        try
        {
            MikrotikInBop.ConnectAndReboot(_logger, Logs.RtuManager.ToInt(), damagedOtau.Ip, connectionTimeout);
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.RtuManager, $"Cannot connect Mikrotik {damagedOtau.Ip}" + e.Message);
        }

    }

}