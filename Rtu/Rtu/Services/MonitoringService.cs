using Fibertest.Utils;
using System.Diagnostics;

namespace Fibertest.Rtu;

public class MonitoringService : BackgroundService
{
    private readonly ILogger<MonitoringService> _logger;
    private readonly RtuManager _rtuManager;

    public MonitoringService(ILogger<MonitoringService> logger, 
        RtuManager rtuManager)
    {
        _logger = logger;
        _rtuManager = rtuManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;
        _logger.LogInfo(Logs.RtuManager, $"RTU monitoring service started. Process {pid}, thread {tid}");
            
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        await _rtuManager.InitializeRtu();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            _logger.LogDebug(Logs.RtuManager,  "It is a measurement thread ..." + Environment.NewLine);
        }
    }
}