namespace Fibertest.Dto;

public class StopMonitoringDto : BaseRtuRequest
{
    public StopMonitoringDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public override string What => "StopMonitoring";
    public override RtuOccupation Why => RtuOccupation.None;
}