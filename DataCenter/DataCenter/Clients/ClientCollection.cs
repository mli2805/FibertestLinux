using System.Collections.Concurrent;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public class ClientCollection
{
    private readonly ILogger<ClientCollection> _logger;
    public readonly ConcurrentDictionary<string, ClientStation> Clients = new();

    public ClientCollection(ILogger<ClientCollection> logger)
    {
        _logger = logger;
    }

    public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
    {
        await Task.Delay(1);
        // instead of this line many-many checks
        var user = new User(dto.UserName, dto.Password);

        var clientStation = new ClientStation(dto, user) { ClientIp = dto.ClientIp ?? "client IP not set"};
        if (!Clients.TryAdd(clientStation.ConnectionId, clientStation))
            return new ClientRegisteredDto(ReturnCode.Error);
        var successfulResult = this.FillInSuccessfulResult(dto, user);
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Client {clientStation.UserName} from {clientStation.ClientIp} registered successfully!");
        return successfulResult;
    }

    public async Task<RequestAnswer> RegisterHeartbeat(RegisterHeartbeatDto dto)
    {
        await Task.Delay(1);
        if (!Clients.ContainsKey(dto.ConnectionId))
            return new RequestAnswer(ReturnCode.ClientCleanedAsDead);
        Clients[dto.ConnectionId].LastConnectionTimestamp = DateTime.Now;
        return new RequestAnswer(ReturnCode.Ok);
    }

    public async Task<RequestAnswer> UnRegisterClientAsync(UnRegisterClientDto dto)
    {
        await Task.Delay(1);
        if (!Clients.ContainsKey(dto.ConnectionId))
            return new RequestAnswer(ReturnCode.ClientCleanedAsDead);
        return Clients.TryRemove(dto.ConnectionId, out var _)
            ? new RequestAnswer(ReturnCode.Ok)
            : new RequestAnswer(ReturnCode.Error);
    }
}