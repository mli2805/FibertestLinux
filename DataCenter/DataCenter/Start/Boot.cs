using System.Diagnostics;
using System.Reflection;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public sealed class Boot : IHostedService
{
    private readonly ILogger<Boot> _logger;
    private readonly IDbInitializer _mySqlDbInitializer;
    private readonly EventStoreService _eventStoreService;

    public Boot(ILogger<Boot> logger, IDbInitializer mySqlDbInitializer, EventStoreService eventStoreService)
    {
        _logger = logger;
        _mySqlDbInitializer = mySqlDbInitializer;
        _eventStoreService = eventStoreService;
    }

    // Place here all that should be done before start listening to gRPC & Http requests, background workers, etc.
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;

        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), Environment.NewLine + Environment.NewLine + new string('-', 78));
        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Fibertest Data-Center {info.FileVersion}. Process {pid}, thread {tid}");


        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), _mySqlDbInitializer.ConnectionLogLine);
        await _eventStoreService.InitializeBothDb();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "Leave Fibertest Data-Center service");
        return Task.CompletedTask;
    }
}