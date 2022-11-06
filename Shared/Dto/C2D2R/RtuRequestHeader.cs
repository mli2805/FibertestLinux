namespace Fibertest.Dto;

public class RtuRequestHeader
{
    public string ConnectionId;
    public Guid RtuId;
    public RtuMaker RtuMaker;

    public RtuRequestHeader(string connectionId, Guid rtuId, RtuMaker rtuMaker)
    {
        ConnectionId = connectionId;
        RtuId = rtuId;
        RtuMaker = rtuMaker;
    }
}