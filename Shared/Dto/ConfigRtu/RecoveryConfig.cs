namespace Fibertest.Dto;

public class RecoveryConfig
{
    public int MikrotikRebootTimeout { get; set; } = 45;
    public int MikrotikRebootAttemptsBeforeNotification { get; set; } = 3;
    public RecoveryStep RecoveryStep { get; set; }
    public bool RebootSystemEnabled { get; set; }
    public int RebootSystemDelay { get; set; } = 60;
}