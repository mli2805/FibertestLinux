using System.Diagnostics;
using System.Reflection;
using Fibertest.Dto;
using Fibertest.Utils;
using Serilog.Events;

namespace Fibertest.DataCenter;

public sealed class Boot : IHostedService
{
    private readonly IWritableConfig<DataCenterConfig> _config;
    private readonly ILogger<Boot> _logger;
    private readonly EventStoreService _eventStoreService;

    public Boot(IWritableConfig<DataCenterConfig> config, ILogger<Boot> logger, EventStoreService eventStoreService)
    {
        _config = config;
        _logger = logger;
        _eventStoreService = eventStoreService;
    }

    // Place here all that should be done before start listening to gRPC & Http requests, background workers, etc.
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;

        _logger.StartLine(Logs.DataCenter);
        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
        _config.Update(c=>c.General.DatacenterVersion = info.FileVersion! + $"  checked at {DateTime.Now}");
        _logger.LogInfo(Logs.DataCenter, $"Fibertest Data-Center {info.FileVersion}. Process {pid}, thread {tid}");

        _config.Update(c=>c.General.LogEventLevel = LogEventLevel.Debug.ToString());

        await _eventStoreService.InitializeBothDb();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInfo(Logs.DataCenter, "Leave Fibertest Data-Center service");
        return Task.CompletedTask;
    }
}