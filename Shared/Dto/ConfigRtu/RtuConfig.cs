namespace Fibertest.Dto;

public class RtuConfig
{
    public RtuGeneralConfig General { get; set; } = new RtuGeneralConfig();
    public MonitoringConfig Monitoring { get; set; } = new MonitoringConfig();
    public CharonConfig Charon { get; set; } = new CharonConfig();
    public RecoveryConfig Recovery { get; set; } = new RecoveryConfig();
}