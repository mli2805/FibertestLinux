namespace Fibertest.DataCenter
{
    public class DataCenterConfig
    {
        public int EventSourcingPortion { get; set; }
        public int CheckHeartbeatEvery { get; set; }
        public int RtuHeartbeatPermittedGap { get; set; }
        public int ClientConnectionsPermittedGap { get; set; }
    }
}
