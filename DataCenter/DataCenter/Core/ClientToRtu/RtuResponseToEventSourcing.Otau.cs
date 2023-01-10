using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter
{
    public partial class RtuResponseToEventSourcing
    {
        public async Task<string?> ApplyOtauAttachmentResult(AttachOtauDto dto, OtauAttachedDto result)
        {
            var client = _clientCollection.Get(dto.ClientConnectionId);
            if (client == null) return "client not found";

            var cmd = new AttachOtau
            {
                Id = result.OtauId,
                RtuId = dto.RtuId,
                MasterPort = dto.OpticalPort,
                Serial = result.Serial,
                PortCount = result.PortCount,
                NetAddress = dto.NetAddress.Clone(),
                IsOk = true,
            };

            return await _eventStoreService.SendCommand(cmd, client.UserName, client.ClientIp);
        }

        public async Task<string?> ApplyOtauDetachmentResult(DetachOtauDto dto)
        {
            var client = _clientCollection.Get(dto.ClientConnectionId);
            if (client == null) return "client not found";

            var otau = _writeModel.Otaus.FirstOrDefault(o => o.Id == dto.OtauId);
            if (otau == null) return null;
            var cmd = new DetachOtau
            {
                Id = dto.OtauId,
                RtuId = dto.RtuId,
                OtauIp = dto.NetAddress.Ip4Address,
                TcpPort = dto.NetAddress.Port,
                TracesOnOtau = _writeModel.Traces
                    .Where(t => t.OtauPort != null && t.OtauPort.Serial == otau.Serial)
                    .Select(t => t.TraceId)
                    .ToList(),
            };

            return await _eventStoreService.SendCommand(cmd, client.UserName, client.ClientIp);
        }
    }
}
