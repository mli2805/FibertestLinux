using Fibertest.Dto;

namespace Fibertest.Rtu;

public class RtuConfig
{
    public int RtuHeartbeatRate { get; set; }

    public DoubleAddress? ServerAddress { get; set; }
    public Guid RtuId { get; set; }


    public int RtuPauseAfterReboot { get; set; }
    public int RtuUpTimeForAdditionalPause { get; set; }
}

public class MonitoringConfig
{
    public bool IsMonitoringOn { get; set; }
}