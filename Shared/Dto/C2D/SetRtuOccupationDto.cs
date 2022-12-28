namespace Fibertest.Dto;

public class SetRtuOccupationDto : BaseRequest
{
    public Guid RtuId;
    public RtuOccupationState? State;

    public override string What => "SetRtuOccupation";

}