namespace Fibertest.Dto
{
    public class RtuMonitoringSettingsDto
    {
        public string? ConnectionId;
        public string? RtuTitle;
        public RtuMaker RtuMaker;
        public string? OtdrId;
        // public string? OtauId;
        public VeexOtau MainVeexOtau = new VeexOtau(); // in Veex RTU it is a separate unit
        public MonitoringState MonitoringMode;

        public Frequency PreciseMeas;
        public Frequency PreciseSave;
        public Frequency FastSave;

        public List<RtuMonitoringPortDto>? Lines;
    }
}