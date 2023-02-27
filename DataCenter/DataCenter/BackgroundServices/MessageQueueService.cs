using System.Diagnostics;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public class MessageQueueService : BackgroundService
{
    private readonly ILogger<MessageQueueService> _logger;

    public MessageQueueService(ILogger<MessageQueueService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;
        _logger.Info(Logs.DataCenter, $"Rabbit Message queue service started. Process {pid}, thread {tid}");
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(3000, stoppingToken);
        }
    }
}