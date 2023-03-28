using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KadastrLoader
{
    public class KadastrDbSettings
    {
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly ILogger _logger;
        private string _mysqlServerAddress = null!;
        private int _mysqlTcpPort;

        public KadastrDbSettings(IWritableConfig<ClientConfig> config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Init()
        {
            var doubleAddress = _config.Value.General.ServerAddress;
            _mysqlServerAddress = doubleAddress.Main.IsAddressSetAsIp
                ? doubleAddress.Main.Ip4Address
                : doubleAddress.Main.HostName;

            _mysqlTcpPort = _config.Value.General.MysqlTcpPort;
            _logger.Info(Logs.Client, $"MySqlConnectionString = {MySqlConnectionString}");
        }
        // private string MySqlConnectionString => $"server=localhost;port={_mysqlTcpPort};user id=root;password=root;database=ft30kadastr";
       private string MySqlConnectionString => $"server={_mysqlServerAddress};port={_mysqlTcpPort};user id=root;password=root;database=ft30kadastr";

        public DbContextOptions<KadastrDbContext> Options =>
            new DbContextOptionsBuilder<KadastrDbContext>()
                .UseMySql(MySqlConnectionString, ServerVersion.AutoDetect(MySqlConnectionString)).Options;
    }
}