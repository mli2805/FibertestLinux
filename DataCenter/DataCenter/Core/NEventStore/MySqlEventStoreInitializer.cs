using System.Data.Common;
using Fibertest.Utils;
using MySqlConnector;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;

namespace Fibertest.DataCenter
{
    public sealed class MySqlEventStoreInitializer : IEventStoreInitializer
    {
        private readonly ILogger<MySqlEventStoreInitializer> _logger;

        private readonly int _mysqlTcpPort;
        private readonly string _eventSourcingScheme;
        public string? DataDir { get ; private set; }
        public string ConnectionString { get; private set; }

        public MySqlEventStoreInitializer(IWritableOptions<MysqlConfig> config, ILogger<MySqlEventStoreInitializer> logger)
        {
            _logger = logger;
            _mysqlTcpPort = config.Value.TcpPort; // default 3306
            var postfix = config.Value.SchemePostfix;
            _eventSourcingScheme = "ft30graph" + postfix;

            ConnectionString = $"server=localhost;port={_mysqlTcpPort};user id=root;password=root;";
        }

        public IStoreEvents Init()
        {
            CreateDatabaseIfNotExists();
            try
            {
                var providerFactory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
                var eventStore = Wireup.Init()
                    .UsingSqlPersistence(providerFactory, $"{ConnectionString}database={_eventSourcingScheme}")
                    .WithDialect(new MySqlDialect())
                    .InitializeStorageEngine()
                    .Build();

                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Events store: MYSQL=localhost:{_mysqlTcpPort}   Database={_eventSourcingScheme}");

                InitializeDataDir();
                return eventStore;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "MySqlEventStoreInitializer exception : " + e.Message);
                throw;
            }
        }

        private void InitializeDataDir()
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
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
                MySqlConnection connection = new MySqlConnection(ConnectionString);
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
                MySqlConnection connection = new MySqlConnection(ConnectionString);
                connection.Open();
                MySqlCommand command1 = new MySqlCommand($"SELECT CheckPointNumber FROM ft30graph.commits WHERE StreamRevision = {lastEventNumber};", connection);
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
                MySqlConnection connection = new MySqlConnection(ConnectionString);
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
                MySqlConnection connection = new MySqlConnection(ConnectionString);
                MySqlCommand command = new MySqlCommand($"drop database if exists {_eventSourcingScheme};", connection);
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
                MySqlConnection connection = new MySqlConnection(ConnectionString);
                MySqlCommand command = new MySqlCommand($"create database if not exists {_eventSourcingScheme};", connection);
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
                MySqlConnection connection = new MySqlConnection(ConnectionString);
                MySqlCommand command = new MySqlCommand(
                    "SELECT StreamIdOriginal FROM ft20graph.commits", connection);
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