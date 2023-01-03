using System.Data.Common;
using Fibertest.Utils;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;

namespace Fibertest.DataCenter
{
    public sealed class MySqlDbInitializer : IDbInitializer
    {
        private readonly ILogger<MySqlDbInitializer> _logger;
        private readonly MySerializer _mySerializer;

        private readonly int _mysqlTcpPort;
        private readonly string _postfix;
        private string EventSourcingScheme => "ft30graph" + _postfix;
        private string OtherTablesScheme => "ft30efcore" + _postfix;

        public string ConnectionLogLine =>
            $"MYSQL=localhost:{_mysqlTcpPort}   Database={EventSourcingScheme} / {OtherTablesScheme}";

        // for "other" tables (not event sourcing)
        public DbContextOptions<FtDbContext> FtDbContextOptions =>
            new DbContextOptionsBuilder<FtDbContext>()
                .UseMySql(FtConnectionString, ServerVersion.AutoDetect(FtConnectionString)).Options;

        public string? DataDir { get; private set; }
        private string FtConnectionString => $"server=localhost;port={_mysqlTcpPort};user id=root;password=root;database={OtherTablesScheme}";
        public string EsConnectionString => $"server=localhost;port={_mysqlTcpPort};user id=root;password=root;database={EventSourcingScheme}";

        public MySqlDbInitializer(IWritableOptions<MysqlConfig> config, ILogger<MySqlDbInitializer> logger, MySerializer mySerializer)
        {
            _logger = logger;
            _mySerializer = mySerializer;
            _mysqlTcpPort = config.Value.TcpPort; // default 3306
            _postfix = config.Value.SchemePostfix;
        }

        public IStoreEvents Init()
        {
            CreateDatabaseIfNotExists();
            try
            {
                DbProviderFactories.RegisterFactory("AnyNameYouWant", MySqlConnectorFactory.Instance);
                var providerFactory = DbProviderFactories.GetFactory("AnyNameYouWant");

                var eventStore = Wireup.Init()
                    .UsingSqlPersistence(providerFactory, $"{EsConnectionString}")
                    .WithDialect(new MySqlDialect())
                    .InitializeStorageEngine()
                    .UsingCustomSerialization(_mySerializer)
                    .Build();

                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(),
                    $"Events store: MYSQL=localhost:{_mysqlTcpPort}   Database={EventSourcingScheme}");

                InitializeDataDir();
                return eventStore;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "MySqlDbInitializer exception : " + e.Message);
                throw;
            }
        }

        private void InitializeDataDir()
        {
            MySqlConnection connection = new MySqlConnection(FtConnectionString);
            MySqlCommand command = new MySqlCommand("select @@datadir", connection);
            connection.Open();
            DataDir = (string)(command.ExecuteScalar() ?? 0);
            connection.Close();
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"MySQL data folder is {DataDir}");
        }

        public int OptimizeSorFilesTable()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(FtConnectionString);
                MySqlCommand command = new MySqlCommand("optimize table ft30efcore.sorfiles", connection);
                connection.Open();
                var unused = command.ExecuteNonQuery();
                connection.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                return unused;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "OptimizeSorFilesTable: " + e.Message);
                return -1;
            }
        }

        public int RemoveCommitsIncludedIntoSnapshot(int lastEventNumber)
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(FtConnectionString);
                connection.Open();
                MySqlCommand command1 = new MySqlCommand($"SELECT CheckPointNumber FROM ft30graph.Commits WHERE StreamRevision = {lastEventNumber};", connection);
                var checkPointNumber = (long)(command1.ExecuteScalar() ?? 0 + 1);

                MySqlCommand command = new MySqlCommand($"DELETE FROM ft20graph.commits WHERE CheckPointNumber < {checkPointNumber};", connection);
                command.ExecuteNonQuery();
                connection.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                return 1;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), e.Message);
                return -1;
            }
        }


        public long GetDataSize()
        {
            var l1 = GetSchemaSize("\"ft30efcore\"");
            var l2 = GetSchemaSize("\"ft30graph\"");
            return l1 + l2;
        }

        private long GetSchemaSize(string schema)
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(FtConnectionString);
                MySqlCommand command = new MySqlCommand(
                    $"SELECT SUM(data_length + index_length) FROM information_schema.tables WHERE table_schema = {schema}", connection);
                connection.Open();
                var result = (decimal)(command.ExecuteScalar() ?? 0);
                connection.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                return (long)result;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "GetDataSize: " + e.Message);
                return -1;
            }

        }

        public void DropDatabase()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(FtConnectionString);
                MySqlCommand command = new MySqlCommand($"drop database if exists {EventSourcingScheme};", connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), e.Message);
                throw;
            }
        }

        private void CreateDatabaseIfNotExists()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(FtConnectionString);
                MySqlCommand command = new MySqlCommand($"create database if not exists {EventSourcingScheme};", connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), e.Message);
                throw;
            }
        }

        public Guid GetStreamIdIfExists()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(FtConnectionString);
                MySqlCommand command = new MySqlCommand(
                    "SELECT StreamIdOriginal FROM ft30graph.Commits", connection);
                connection.Open();
                var result = (string)(command.ExecuteScalar() ?? 0);
                connection.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                return Guid.Parse(result);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "GetStreamIdIfExists: " + e.Message);
                return Guid.Empty;
            }

        }
    }
}