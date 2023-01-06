using System.Diagnostics;
using System.Reflection;
using Fibertest.Utils;

namespace Fibertest.Rtu;

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
        _logger.StartLine(Logs.RtuService.ToInt());
        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
        _logger.LLog(Logs.RtuService.ToInt(), $"Fibertest RTU service {info.FileVersion}");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LLog(Logs.RtuService.ToInt(), "Leave Fibertest RTU service");
        return Task.CompletedTask;
    }
}