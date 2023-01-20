namespace Fibertest.Dto;

public class UnRegisterClientDto : BaseRequest
{
    public string? UserName;

    public UnRegisterClientDto(string userName)
    {
        UserName = userName;
    }

    public override string What => "UnRegisterClient";
}