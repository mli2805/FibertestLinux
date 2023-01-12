using System.Diagnostics;
using System.Reflection;
using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.Extensions.Options;

namespace Fibertest.DataCenter;

public sealed class Boot : IHostedService
{
    private readonly IOptions<ServerGeneralConfig> _generalConfig;
    private readonly ILogger<Boot> _logger;
    private readonly EventStoreService _eventStoreService;

    public Boot(IOptions<ServerGeneralConfig> generalConfig, ILogger<Boot> logger, EventStoreService eventStoreService)
    {
        _generalConfig = generalConfig;
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
        _logger.LLog(Logs.DataCenter, $"Fibertest Data-Center {info.FileVersion}. Process {pid}, thread {tid}");

        _logger.LLog(Logs.DataCenter, 
            $"Minimum log level set as {LoggerConfigurationFactory.Parse(_generalConfig.Value.LogLevel)}");

        await _eventStoreService.InitializeBothDb();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LLog(Logs.DataCenter, "Leave Fibertest Data-Center service");
        return Task.CompletedTask;
    }
}