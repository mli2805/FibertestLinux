namespace Fibertest.Dto;

public class ClientRegisteredDto : RequestAnswer
{
    public string? ConnectionId;

    public Guid UserId;
    public Role Role;
    public Guid ZoneId;
    public string? ZoneTitle;
        
    public bool IsWithoutMapMode;

    public CurrentDatacenterParameters? CurrentDatacenterParameters;
        

    public ClientRegisteredDto(ReturnCode returnCode) : base(returnCode)
    {
    }
}