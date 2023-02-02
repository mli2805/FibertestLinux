namespace Fibertest.Dto;

public class ApplyMonitoringSettingsDto : BaseRtuRequest
{
    public ApplyMonitoringSettingsDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public string? OtdrId; //  in VeEX RTU main OTDR has its own ID

    public VeexOtau MainVeexOtau = new VeexOtau(); // in Veex RTU it is a separate unit

    public bool IsMonitoringOn;

    public MonitoringTimespansDto Timespans = new MonitoringTimespansDto();

    public List<PortWithTraceDto> Ports = new List<PortWithTraceDto>();

    public override string What => "ApplyMonitoringSettings";
    public override RtuOccupation Why => RtuOccupation.ApplyMonitoringSettings;
}