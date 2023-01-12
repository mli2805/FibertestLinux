using System.Diagnostics;
using System.Reflection;
using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Newtonsoft.Json;

namespace Fibertest.Rtu;

public class HeartbeatService : BackgroundService
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };
    private readonly IWritableOptions<RtuGeneralConfig> _config;
    private readonly ILogger<HeartbeatService> _logger;
    private readonly RtuManager _rtuManager;

    private string _version = "";
    private bool _isLastAttemptSuccessful;
    private bool _initializationInProgress;

    public HeartbeatService(IWritableOptions<RtuGeneralConfig> config, ILogger<HeartbeatService> logger,
        RtuManager rtuManager)
    {
        _config = config;
        _logger = logger;
        _rtuManager = rtuManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;

        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
        _version = info.FileVersion ?? "Unknown";

        _logger.LogInfo(Logs.RtuService, $"RTU heartbeat service started. Process {pid}, thread {tid}");
        _logger.LogInfo(Logs.RtuService,
            $"Server address is {_config.Value.ServerAddress.Main.ToStringA()}");
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (!_rtuManager.ShouldSendHeartbeat.TryPeek(out object? _))
                {
                    if (!_initializationInProgress)
                        _logger.LogInfo(Logs.RtuService, "Heartbeats are suspended during RTU initialization.");

                    _initializationInProgress = true;
                    Thread.Sleep(3000);
                }

                _initializationInProgress = false;
                _isLastAttemptSuccessful = await SendHeartbeat();

                var rtuHeartbeatRate = _config.Value.RtuHeartbeatRate == 0 ? 30 : _config.Value.RtuHeartbeatRate;
                await Task.Delay(rtuHeartbeatRate * 1000, stoppingToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.RtuService, "Heartbeat service DoWork: " + e.Message);
        }
    }

    private async Task<bool> SendHeartbeat()
    {
        try
        {
            var serverAddress = _config.Value.ServerAddress;

            var dto = new RtuChecksChannelDto(_config.Value.RtuId, _version, true);
            var command = new R2DGrpcCommand() { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

            var dcUri = $"http://{serverAddress.Main.ToStringA()}";
            _logger.LogInfo(Logs.RtuService, "SendHeartbeat: " + dcUri);
            using var grpcChannelDc = GrpcChannel.ForAddress(dcUri);
            var grpcClient = new R2D.R2DClient(grpcChannelDc);

            R2DGrpcResponse response = await grpcClient.SendCommandAsync(command);
            if (!_isLastAttemptSuccessful)
                _logger.LogInfo(Logs.RtuService, $"Got gRPC response {response.Json} from Data Center");
            else
                _logger.LogInfo(Logs.RtuService, $"RTU heartbeat sent by gRPC channel {dcUri}");

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.RtuService, "SendHeartbeat: " + e.Message);
            if (e.InnerException != null)
                _logger.LogError(Logs.RtuService, "SendHeartbeat: " + e.InnerException.Message);
            return false;
        }
    }
}