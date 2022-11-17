using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public class RtuRepo
    {
        private readonly IWritableOptions<MysqlConfig> _config;
        private readonly ILogger<RtuRepo> _logger;
        public RtuRepo(IWritableOptions<MysqlConfig> config, ILogger<RtuRepo> logger)
        {
            _config = config;
            _logger = logger;
        }

        // Read it from DB
        public RtuStation? Get(Guid rtuId)
        {
            var tcpPort = _config.Value.TcpPort;
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"TCP port in datacenter.json is {tcpPort}");
            _config.Update(o => o.TcpPort = 3306);

            var newTcpPort = _config.Value.TcpPort;
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"And now TCP port is {newTcpPort}");

            if (rtuId == Guid.Empty) return null;
            return new RtuStation()
            {
                RtuGuid = rtuId,
                MainAddress = "192.168.96.56",
                MainAddressPort = 11942,
                IsMainAddressOkDuePreviousCheck = true,
            };
        }

    }
}
