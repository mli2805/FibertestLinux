namespace Fibertest.Dto;

public class StopMonitoringDto : RtuRequestHeader
{
    public StopMonitoringDto(string connectionId, Guid rtuId, RtuMaker rtuMaker) : base(connectionId, rtuId, rtuMaker)
    {
    }
}