using Fibertest.Utils;
using System.Diagnostics;
using Fibertest.Dto;

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
        _logger.Info(Logs.RtuManager, $"RTU monitoring service started. Process {pid}, thread {tid}");
            
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        var result = await _rtuManager.InitializeRtu();
        if (result.ReturnCode == ReturnCode.RtuInitializedSuccessfully)
            await _rtuManager.RunMonitoringCycle();

        //
        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        //     _logger.Debug(Logs.RtuManager,  "It is a measurement thread ..." + Environment.NewLine);
        // }
    }
}