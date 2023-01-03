using System.Diagnostics;
using System.Reflection;
using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Newtonsoft.Json;

namespace Fibertest.Rtu
{
    public class HeartbeatService : BackgroundService
    {
        private readonly IWritableOptions<RtuGeneralConfig> _config;
        private readonly ILogger<HeartbeatService> _logger;

        private string _version = "";
        private bool _isLastAttemptSuccessful;

        public HeartbeatService(IWritableOptions<RtuGeneralConfig> config, ILogger<HeartbeatService> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;

            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
            _version = info.FileVersion ?? "Unknown";

            _logger.Log(LogLevel.Information, Logs.RtuService.ToInt(), $"RTU heartbeat service started. Process {pid}, thread {tid}");
            _logger.Log(LogLevel.Information, Logs.RtuService.ToInt(),
                $"Server address is {_config.Value.ServerAddress.Main.ToStringA()}");
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _isLastAttemptSuccessful = await SendHeartbeat();

                var rtuHeartbeatRate = _config.Value.RtuHeartbeatRate == 0 ? 30 : _config.Value.RtuHeartbeatRate;
                await Task.Delay(rtuHeartbeatRate * 1000, stoppingToken);
            }
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };
        private async Task<bool> SendHeartbeat()
        {
            try
            {
                var serverAddress = _config.Value.ServerAddress;
               
                var dto = new RtuChecksChannelDto(_config.Value.RtuId, _version, true);
                var command = new R2DGrpcCommand() { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

                var dcUri = $"http://{serverAddress.Main.ToStringA()}";
                using var grpcChannelDc = GrpcChannel.ForAddress(dcUri);
                var grpcClient = new R2D.R2DClient(grpcChannelDc);
              
                R2DGrpcResponse response = await grpcClient.SendCommandAsync(command);
                if (!_isLastAttemptSuccessful)
                    _logger.Log(LogLevel.Information, Logs.RtuService.ToInt(), $"Got gRPC response {response.Json} from Data Center");
                else
                    _logger.Log(LogLevel.Information, Logs.RtuService.ToInt(), $"RTU heartbeat sent by gRPC channel {dcUri}");

                return true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.RtuService.ToInt(), "SendHeartbeat: " + e.Message);
                if (e.InnerException != null)
                    _logger.Log(LogLevel.Error, Logs.RtuService.ToInt(), "SendHeartbeat: " + e.InnerException.Message);
                return false;
            }
        }
    }
}
