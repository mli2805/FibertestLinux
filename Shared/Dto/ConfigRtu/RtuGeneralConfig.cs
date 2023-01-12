namespace Fibertest.Dto;

public class RtuGeneralConfig
{
    public int RtuHeartbeatRate { get; set; } = 30;

    public DoubleAddress ServerAddress { get; set; } = new DoubleAddress();
    public Guid RtuId { get; set; }
    public int PreviousOwnPortCount { get; set; } = -1;
    public string CharonIp { get; set; } = "192.168.88.101";
    public int OtdrTcpPort { get; set; } = 1500;


    public int RtuPauseAfterReboot { get; set; } = 20;
    public int RtuUpTimeForAdditionalPause { get; set; } = 100;

    public string LogLevel { get; set; } = "2";
}