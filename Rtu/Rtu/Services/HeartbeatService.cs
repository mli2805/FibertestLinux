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
        private readonly IWritableOptions<RtuConfig> _config;
        private readonly ILogger<HeartbeatService> _logger;

        private string? _version;
        private bool _isLastAttemptSuccessful;

        public HeartbeatService(IWritableOptions<RtuConfig> config, ILogger<HeartbeatService> logger)
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
            _version = info.FileVersion;

            _logger.Log(LogLevel.Information, Logs.RtuService.ToInt(), $"RTU heartbeat service started. Process {pid}, thread {tid}");
            _logger.Log(LogLevel.Information, Logs.RtuService.ToInt(),
                $"Server address is {_config.Value.ServerAddress?.Main.ToStringA() ?? "null"}");
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
                if (serverAddress == null)
                {
                    if (_isLastAttemptSuccessful)
                        _logger.Log(LogLevel.Error, Logs.RtuService.ToInt(), "Data Center address not found.");
                    return false;
                }

                var dcUri = $"http://{serverAddress.Main.ToStringA()}";
                _logger.Log(LogLevel.Debug, Logs.RtuService.ToInt(), $"RTU heartbeat: uri is {dcUri}");
                using var grpcChannelDc = GrpcChannel.ForAddress(dcUri);
                _logger.Log(LogLevel.Debug, Logs.RtuService.ToInt(), $"RTU heartbeat: gRPC channel to Data-Center {dcUri}");
                var grpcClient = new R2D.R2DClient(grpcChannelDc);

                var rtuId = _config.Value.RtuId;
                var dto = new RtuChecksChannelDto() { RtuId = rtuId, IsMainChannel = true, Version = _version };
                var command = new R2DGrpcCommand() { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };


                R2DGrpcResponse response = await grpcClient.SendCommandAsync(command);
                if (!_isLastAttemptSuccessful)
                    _logger.Log(LogLevel.Information, Logs.RtuService.ToInt(), $"Got gRPC response {response.Json} from Data Center");
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
