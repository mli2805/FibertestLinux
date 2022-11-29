using System.Diagnostics;
using System.Reflection;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public sealed class Boot : IHostedService
{
    private readonly ILogger<Boot> _logger;

    public Boot(ILogger<Boot> logger)
    {
        _logger = logger;
    }

    // Place here all that should be done before start listening to gRPC & Http requests, background workers, etc.
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), Environment.NewLine + Environment.NewLine + new string('-', 80));
        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Fibertest Data-Center {info.FileVersion}");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "Leave Fibertest Data-Center service");
        return Task.CompletedTask;
    }
}