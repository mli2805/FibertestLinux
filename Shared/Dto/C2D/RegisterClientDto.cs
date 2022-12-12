namespace Fibertest.Dto;

public class RegisterClientDto : BaseRequest
{
    public string? ClientIp;
    public DoubleAddress? Addresses;
    public string UserName;
    public string Password;
    public string? MachineKey;
    public string? SecurityAdminPassword; // Hashed
    public bool IsUnderSuperClient;
    public bool IsWebClient;

    public RegisterClientDto(string connectionId, string username, string password) : base(connectionId)
    {
        UserName = username;
        Password = password;
    }

    public override string What => "RegisterClient";

}

public class UnRegisterClientDto : BaseRequest
{
    public UnRegisterClientDto(string connectionId, string userName) : base(connectionId)
    {
        UserName = userName;
    }

    public string? UserName;
}