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

        var clientStation = new ClientStation(dto, user) { ClientIp = dto.ClientIp ?? "client IP not set" };
        if (!Clients.TryAdd(clientStation.ConnectionId, clientStation))
            return new ClientRegisteredDto(ReturnCode.Error);
        var successfulResult = this.FillInSuccessfulResult(dto, user);
        _logger.LLog(Logs.DataCenter.ToInt(), 
            $"Client {clientStation.UserName} from {clientStation.ClientIp} registered successfully!");
        LogStations();
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

    public List<KeyValuePair<string, string>> CleanDeadClients(TimeSpan timeSpan)
    {
        var commands = new List<KeyValuePair<string, string>>();
        DateTime noLaterThan = DateTime.Now - timeSpan;
        var deadStations = Clients.Values.Where(s => s.LastConnectionTimestamp < noLaterThan).ToList();
        if (deadStations.Count == 0) return commands;

        foreach (var deadStation in deadStations)
        {
            _logger.LLog(Logs.DataCenter.ToInt(),
                $"Dead client {deadStation} with connectionId {deadStation.ConnectionId} and last checkout time {deadStation.LastConnectionTimestamp:T} removed.");

            var command = new KeyValuePair<string, string>(deadStation.UserName, deadStation.ClientIp);
            commands.Add(command);
            //await _eventStoreService.SendCommand(command, deadStation.UserName, deadStation.ClientIp);

            Clients.TryRemove(deadStation.ConnectionId, out _);
        }
        LogStations();
        return commands;
    }

    private void LogStations()
    {
        _logger.HyphenLine(Logs.DataCenter.ToInt());
        _logger.LLog(Logs.DataCenter.ToInt(), $"There are {Clients.Count} client(s):");
        _logger.HyphenLine(Logs.RtuService.ToInt());
        foreach (var station in Clients.Values)
        {
            _logger.LLog(Logs.DataCenter.ToInt(),
                $"{station.UserName}/{station.ClientIp}:{station.ClientAddressPort} with connection id {station.ConnectionId}");
        }
        _logger.HyphenLine(Logs.DataCenter.ToInt());
    }
}