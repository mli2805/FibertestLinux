using Fibertest.Utils;
using System.Diagnostics;
using Fibertest.Dto;

namespace Fibertest.Rtu;

public class MonitoringService : BackgroundService
{
    private readonly IWritableConfig<RtuConfig> _config;
    private readonly ILogger<MonitoringService> _logger;
    private readonly RtuManager _rtuManager;

    public MonitoringService(IWritableConfig<RtuConfig> config, ILogger<MonitoringService> logger, 
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
        _logger.Info(Logs.RtuManager, $"RTU monitoring service started. Process {pid}, thread {tid}");
            
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        _rtuManager.RtuServiceCancellationToken = stoppingToken;
        var result = await _rtuManager.InitializeRtu(null, !_config.Value.Monitoring.IsMonitoringOnPersisted);
        if (result.ReturnCode != ReturnCode.RtuInitializedSuccessfully)
            return;
        if (_config.Value.Monitoring.IsMonitoringOnPersisted)
            await _rtuManager.RunMonitoringCycle();

    }
}