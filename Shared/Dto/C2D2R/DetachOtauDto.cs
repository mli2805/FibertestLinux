namespace Fibertest.Dto;

public class DetachOtauDto : BaseRtuRequest
{
    public DetachOtauDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public Guid OtauId;
    public NetAddress NetAddress = null!;
    public int OpticalPort;

    public override string What => "DetachOtau";
    public override RtuOccupation Why => RtuOccupation.DetachOtau;
}