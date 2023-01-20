using System.Diagnostics;
using System.Reflection;
using Fibertest.Dto;
using Fibertest.Utils;
using Fibertest.Utils.Setup;
using Microsoft.Extensions.Options;

namespace Fibertest.Rtu;

public sealed class Boot : IHostedService
{
    private readonly IOptions<RtuGeneralConfig> _generalConfig;
    private readonly ILogger<Boot> _logger;

    public Boot(IOptions<RtuGeneralConfig> generalConfig, ILogger<Boot> logger)
    {
        _generalConfig = generalConfig;
        _logger = logger;
    }

    // Place here all that should be done before start listening to gRPC & Http requests, background workers, etc.
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);

        _logger.StartLine(Logs.RtuService);
        _logger.StartLine(Logs.RtuManager);
        _logger.LogInfo(Logs.RtuService, $"Fibertest RTU service {info.FileVersion}");
        _logger.LogInfo(Logs.RtuManager, $"Fibertest RTU service {info.FileVersion}");

        var configFile = FileOperations.GetMainFolder() +"/config/rtu.json";
        _logger.LogInfo(Logs.RtuService, $"config file: {configFile}");
        _logger.LogInfo(Logs.RtuService, 
            $"Minimum log level set as {LoggerConfigurationFactory.Parse(_generalConfig.Value.LogLevel)}");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInfo(Logs.RtuService, "Leave Fibertest RTU service");
        return Task.CompletedTask;
    }
}