namespace Fibertest.Dto;

public class SetRtuOccupationDto : BaseRequest
{
    public Guid RtuId;
    public RtuOccupationState? State;

    public SetRtuOccupationDto(string connectionId) : base(connectionId)
    {
    }

    public override string What => "SetRtuOccupation";

}