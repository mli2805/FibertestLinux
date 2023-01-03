namespace Fibertest.DataCenter;

public class ServerTimeoutConfig
{
    public int CheckHeartbeatEvery { get; set; } = 30;
    public int RtuHeartbeatPermittedGap { get; set; } = 70;
    public int ClientConnectionsPermittedGap { get; set; } = 180;
}