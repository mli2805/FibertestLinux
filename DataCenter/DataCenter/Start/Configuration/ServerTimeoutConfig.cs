namespace Fibertest.DataCenter;

public class ServerTimeoutConfig
{
    public int CheckHeartbeatEvery { get; set; }
    public int RtuHeartbeatPermittedGap { get; set; }
    public int ClientConnectionsPermittedGap { get; set; }
}