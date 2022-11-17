using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter;

public class ClientStation
{
    // desktop (desktop under superclient) clients put here a GUID produced in WpfClient
    // web clients put here a signalR connectionId
    public string ConnectionId;

    public Guid UserId;
    public string UserName;
    public Role UserRole;

    public string ClientIp;
    public int ClientAddressPort;

    public bool IsUnderSuperClient;
    public bool IsWebClient;
    public bool IsDesktopClient;

    public DateTime LastConnectionTimestamp;

    public ClientStation(RegisterClientDto dto, User user)
    {
        ConnectionId = dto.ConnectionId;

        UserId = user.UserId;
        UserName = dto.UserName ?? "unknown";
        UserRole = user.Role;

        ClientIp = dto.Addresses?.Main.GetAddress() ?? "unknown";
        ClientAddressPort = dto.Addresses?.Main.Port ?? -1;

        IsUnderSuperClient = dto.IsUnderSuperClient;
        IsWebClient = dto.IsWebClient;
        IsDesktopClient = !dto.IsUnderSuperClient && !dto.IsWebClient;

        LastConnectionTimestamp = DateTime.Now;
    }

    public override string ToString()
    {
        return $"{UserName} / {ClientIp}";
    }
}