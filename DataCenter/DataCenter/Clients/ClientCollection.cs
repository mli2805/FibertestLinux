using System.Collections.Concurrent;
using AutoMapper;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public class ClientCollection
{
    public readonly IWritableConfig<DataCenterConfig> Config;
    public readonly ILogger<ClientCollection> Logger;
    public readonly Model WriteModel;
    private readonly EventStoreService _eventStoreService;

    // key is ConnectionId
    public readonly ConcurrentDictionary<string, ClientStation> Clients = new();

    public ClientCollection(IWritableConfig<DataCenterConfig> config, ILogger<ClientCollection> logger, 
        Model writeModel, EventStoreService eventStoreService)
    {
        Config = config;
        Logger = logger;
        WriteModel = writeModel;
        _eventStoreService = eventStoreService;
    }

    public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
    {
        // R1
        var licenseCheckResult = this.CheckLicense(dto);
        if (licenseCheckResult != null)
        {
            Logger.Error(Logs.DataCenter, licenseCheckResult.ReturnCode.GetLocalizedString());
            return licenseCheckResult;
        }

        // R2
        var user = WriteModel.Users
            .FirstOrDefault(u => u.Title == dto.UserName
                                 && u.EncodedPassword == dto.Password);
        if (user == null)
            return new ClientRegisteredDto { ReturnCode = ReturnCode.NoSuchUserOrWrongPassword };

        // R3
        var hasRight = user.CheckRights(dto);
        if (hasRight != null)
            return hasRight;


        // R4
        var theSameUserCheckResult = await this.CheckTheSameUser(dto, user);
        if (theSameUserCheckResult != null)
            return theSameUserCheckResult;

        // R5 Machine Key
        var machineKeyCheckResult = this.CheckMachineKey(dto, user);
        if (machineKeyCheckResult.ReturnCode == ReturnCode.SaveUsersMachineKey)
        {
            IMapper mapper = new MapperConfiguration(
                cfg => cfg.AddProfile<MappingModelToCmdProfile>()).CreateMapper();
            var cmd = mapper.Map<AssignUsersMachineKey>(user);
            await _eventStoreService.SendCommand(cmd, "admin", dto.ClientIp ?? "client IP not set");
        }
        else if (machineKeyCheckResult.ReturnCode != ReturnCode.Ok)
            return machineKeyCheckResult;

        var clientStation = new ClientStation(dto, user)
        {
            ConnectionId = dto.ClientConnectionId,
            ClientIp = dto.ClientIp ?? "client IP not set"
        };
        if (!Clients.TryAdd(clientStation.ConnectionId, clientStation))
            return new ClientRegisteredDto(ReturnCode.Error);
        var successfulResult = this.FillInSuccessfulResult(dto, user);
        Logger.Info(Logs.DataCenter,
            $"Client {clientStation.UserName} from {clientStation.ClientIp} registered successfully!");
        LogStations();
        return successfulResult;
    }

    public async Task<RequestAnswer> RegisterHeartbeat(ClientHeartbeatDto dto)
    {
        await Task.Delay(1);
        if (!Clients.ContainsKey(dto.ClientConnectionId))
            return new RequestAnswer(ReturnCode.ClientCleanedAsDead);
        Clients[dto.ClientConnectionId].LastConnectionTimestamp = DateTime.Now;
        return new RequestAnswer(ReturnCode.Ok);
    }

    public async Task<RequestAnswer> UnRegisterClientAsync(UnRegisterClientDto dto)
    {
        await Task.Delay(1);
        if (!Clients.ContainsKey(dto.ClientConnectionId))
            return new RequestAnswer(ReturnCode.ClientCleanedAsDead);
        return Clients.TryRemove(dto.ClientConnectionId, out var _)
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
            Logger.Info(Logs.DataCenter,
                $@"Dead client {deadStation} with connectionId {
                    deadStation.ConnectionId.Substring(0, 6)} and last checkout time {deadStation.LastConnectionTimestamp:T} removed.");

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
        Logger.HyphenLine(Logs.DataCenter);
        Logger.Info(Logs.DataCenter, $"There are {Clients.Count} client(s):");
        Logger.HyphenLine(Logs.RtuService);
        foreach (var station in Clients.Values)
        {
            Logger.Info(Logs.DataCenter,
                $"{station.UserName}/{station.ClientIp}:{station.ClientAddressPort} with connection id {station.ConnectionId}");
        }
        Logger.HyphenLine(Logs.DataCenter);
    }
}