namespace Fibertest.Dto;

public class AttachOtauDto : RtuRequestHeader
{
    public AttachOtauDto(string connectionId, Guid rtuId, RtuMaker rtuMaker) : base(connectionId, rtuId, rtuMaker)
    {
    }

    public Guid OtauId;
    public NetAddress? NetAddress;
    public int OpticalPort;
}