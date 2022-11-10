using System.Collections.Concurrent;
using Fibertest.Dto;
using Graph;

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
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.Error };
            return this.FillInSuccessfulResult(dto, user);
        }
    }
}
