namespace Fibertest.Dto
{
    public class RegisterClientDto
    {
        public string ConnectionId;
        public string? ClientIp;
        public DoubleAddress? Addresses;
        public string? UserName;
        public string? Password;
        public string? MachineKey;
        public string? SecurityAdminPassword; // Hashed
        public bool IsUnderSuperClient;
        public bool IsWebClient;

        public RegisterClientDto(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
