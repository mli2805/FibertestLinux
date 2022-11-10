namespace Fibertest.Dto;

public class StopMonitoringDto : BaseRtuRequest
{
    public StopMonitoringDto(string connectionId, Guid rtuId, RtuMaker rtuMaker) : base(connectionId, rtuId, rtuMaker)
    {
    }

    public override string What => "StopMonitoring";
    public override RtuOccupation Why() => RtuOccupation.None;
}