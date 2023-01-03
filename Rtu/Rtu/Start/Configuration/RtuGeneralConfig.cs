using Fibertest.Dto;

namespace Fibertest.Rtu;

public class RtuGeneralConfig
{
    public int RtuHeartbeatRate { get; set; }

    public DoubleAddress ServerAddress { get; set; } = new DoubleAddress();
    public Guid RtuId { get; set; }


    public int RtuPauseAfterReboot { get; set; }
    public int RtuUpTimeForAdditionalPause { get; set; }
}