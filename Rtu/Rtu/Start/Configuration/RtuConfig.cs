namespace Fibertest.Rtu;

public class RtuConfig
{
    public string? OtdrIp { get; set; }
    public string? OtauIp { get; set; }

    public int RtuHeartbeatRate { get; set; }
    public int RtuPauseAfterReboot { get; set; }
    public int RtuUpTimeForAdditionalPause { get; set; }
}