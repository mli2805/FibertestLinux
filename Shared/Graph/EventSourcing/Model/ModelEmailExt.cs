using Fibertest.Dto;

namespace Fibertest.Graph
{
    public static class ModelEmailExt
    {
        public static List<string> GetEmailsToSendMonitoringResult(this Model writeModel, MonitoringResultDto dto)
        {
            var trace = writeModel.Traces.FirstOrDefault(t => t.TraceId == dto.PortWithTrace.TraceId);
            if (trace == null) return new List<string>();

            return writeModel.Users.Where(u => u.Email.IsActivated && trace.ZoneIds.Contains(u.ZoneId))
                .Select(u => u.Email.Address).ToList();
        }

        public static List<string> GetEmailsToSendNetworkEvent(this Model writeModel, Guid rtuId)
        {
            var rtu = writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return new List<string>();

            return writeModel.Users.Where(u => u.Email.IsActivated && rtu.ZoneIds.Contains(u.ZoneId))
                .Select(u => u.Email.Address).ToList();
        }

        public static List<string> GetEmailsToSendBopNetworkEvent(this Model writeModel, AddBopNetworkEvent cmd)
        {
            var otau = writeModel.Otaus.FirstOrDefault(o => o.NetAddress.Ip4Address == cmd.OtauIp && o.NetAddress.Port == cmd.TcpPort);
            if (otau == null) return new List<string>();
            var rtu = writeModel.Rtus.FirstOrDefault(r => r.Id == otau.RtuId);
            if (rtu == null) return new List<string>();

            return writeModel.Users.Where(u => u.Email.IsActivated && rtu.ZoneIds.Contains(u.ZoneId))
                .Select(u => u.Email.Address).ToList();
        }
    }
}