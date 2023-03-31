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
    private readonly IWritableConfig<RtuConfig> _config;
    private readonly ILogger<HeartbeatService> _logger;
    private readonly GrpcR2DService _grpcR2DService;
    private readonly RtuManager _rtuManager;

    private string _version = "";
    private bool _isLastAttemptSuccessful;
    private bool _initializationInProgress;

    public HeartbeatService(IWritableConfig<RtuConfig> config, ILogger<HeartbeatService> logger,
        GrpcR2DService grpcR2DService, RtuManager rtuManager)
    {
        _config = config;
        _logger = logger;
        _grpcR2DService = grpcR2DService;
        _rtuManager = rtuManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;

        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
        _version = info.FileVersion ?? "Unknown";

        _logger.Info(Logs.RtuService, $"RTU heartbeat service started. Process {pid}, thread {tid}");
        _logger.Info(Logs.RtuService,
            $"Server address is {_config.Value.General.ServerAddress.Main.ToStringA()}");
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
                        _logger.Info(Logs.RtuService, "Heartbeats are suspended during RTU initialization.");

                    _initializationInProgress = true;
                    Thread.Sleep(3000);
                }

                _initializationInProgress = false;
                _isLastAttemptSuccessful = await SendHeartbeat();

                var rtuHeartbeatRate = _config.Value.General.RtuHeartbeatRate == 0 
                    ? 30 : _config.Value.General.RtuHeartbeatRate;
                await Task.Delay(rtuHeartbeatRate * 1000, stoppingToken);
            }
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, "Heartbeat service DoWork: " + e.Message);
        }
    }

    private async Task<bool> SendHeartbeat()
    {
        try
        {
            var dto = new RtuChecksChannelDto(_config.Value.General.RtuId, _version, true);

            var result = await _grpcR2DService.SendAnyR2DRequest<RtuChecksChannelDto, RequestAnswer>(dto);

            if (_isLastAttemptSuccessful != (result.ReturnCode == ReturnCode.Ok))
            {
                if (result.ReturnCode == ReturnCode.Ok)
                 _logger.Info(Logs.RtuService, "Heartbeat successfully sent to Data Center");
                else
                 _logger.Error(Logs.RtuService, "Failed to send heartbeat to Data Center");
            }

            return result.ReturnCode == ReturnCode.Ok;
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, "SendHeartbeat: " + e.Message);
            if (e.InnerException != null)
                _logger.Error(Logs.RtuService, "SendHeartbeat: " + e.InnerException.Message);
            return false;
        }
    }
}