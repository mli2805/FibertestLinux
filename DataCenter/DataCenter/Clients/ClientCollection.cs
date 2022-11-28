using System.Collections.Concurrent;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter
{
    public class ClientCollection
    {
        public readonly ConcurrentDictionary<string, ClientStation> Clients = new();

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            await Task.Delay(1);
            // instead of this line many-many checks
            var user = new User() { Title = dto.UserName, EncodedPassword = dto.Password };

            var clientStation = new ClientStation(dto, user);
            if (!Clients.TryAdd(clientStation.ConnectionId, clientStation))
                return new ClientRegisteredDto(ReturnCode.Error);
            return this.FillInSuccessfulResult(dto, user);
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
}
