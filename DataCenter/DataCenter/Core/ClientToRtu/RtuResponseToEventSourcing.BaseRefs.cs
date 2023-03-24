using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter
{
    public partial class RtuResponseToEventSourcing
    {
        public async Task ApplyBaseSendingResult(AssignBaseRefsDto dto, AssignBaseRef command)
        {
            var client = _clientCollection.Get(dto.ClientConnectionId);
            if (client == null) return;
            await _eventStoreService.SendCommand(command, client.UserName, client.ClientIp);
        }


        public async Task ApplyBaseSendingResult(AttachTraceDto dto, AttachTrace command)
        {
            var client = _clientCollection.Get(dto.ClientConnectionId);
            if (client == null) return;
            await _eventStoreService.SendCommand(command, client.UserName, client.ClientIp);
        }

        public async Task<string?> ApplyMonitoringSendingResult(ApplyMonitoringSettingsDto dto,
            ChangeMonitoringSettings command)
        {
            return await SendToEventSourcing(dto.ClientConnectionId, command);
        }

        public async Task<string?> SendToEventSourcing(string clientConnectionId, object command)
        {
            var client = _clientCollection.Get(clientConnectionId);
            if (client == null) return "client not found";
            return await _eventStoreService.SendCommand(command, client.UserName, client.ClientIp);
        }
    }
}
