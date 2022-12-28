namespace Fibertest.Dto;

public class AttachOtauDto : BaseRtuRequest
{
    public AttachOtauDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public Guid OtauId;
    public NetAddress? NetAddress;
    public int OpticalPort;

    public override string What => "AttachOtau";
    public override RtuOccupation Why => RtuOccupation.AttachOrDetachOtau;
}