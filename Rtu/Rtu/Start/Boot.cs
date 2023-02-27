using System.Diagnostics;
using System.Reflection;
using Fibertest.Dto;
using Fibertest.Utils;
using Serilog.Events;

namespace Fibertest.Rtu;

public sealed class Boot : IHostedService
{
    private readonly IWritableConfig<RtuConfig> _config;
    private readonly ILogger<Boot> _logger;

    public Boot(IWritableConfig<RtuConfig> config, ILogger<Boot> logger)
    {
        _config = config;
        _logger = logger;
    }

    // Place here all that should be done before start listening to gRPC & Http requests, background workers, etc.
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);

        _logger.StartLine(Logs.RtuService);
        _logger.StartLine(Logs.RtuManager);
        _logger.Info(Logs.RtuService, $"Fibertest RTU service {info.FileVersion}");
        _logger.Info(Logs.RtuManager, $"Fibertest RTU service {info.FileVersion}");

        _config.Update(c=>c.General.LogEventLevel = LogEventLevel.Debug.ToString());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Info(Logs.RtuService, "Leave Fibertest RTU service");
        return Task.CompletedTask;
    }
}