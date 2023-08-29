using Fibertest.Dto;

namespace Fibertest.DataCenter
{
    public partial class Web2DCommandsProcessor
    {
        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            return await _clientCollection.RegisterClientAsync(dto);
        }

        public async Task<RequestAnswer> RegisterHeartbeat(ClientHeartbeatDto dto)
        {
            return await _clientCollection.RegisterHeartbeat(dto);
        }

        public async Task<RequestAnswer> UnRegisterClientAsync(UnRegisterClientDto dto)
        {
            return await _clientCollection.UnRegisterClientAsync(dto);
        }

        public async Task<bool> ChangeGuidWithSignalrConnectionId(string oldGuid, string connId)
        {
            return await _clientCollection.ChangeGuidWithSignalrConnectionId(oldGuid, connId);
        }
    }
}
