using System.Diagnostics;
using System.Reflection;
using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.Extensions.Options;

namespace Fibertest.Rtu;

public class RtuManager
{
    private readonly IOptions<MonitoringConfig> _config;
    private readonly ILogger<RtuManager> _logger;
    private readonly SerialPortManager _serialPortManager;
    private readonly InterOpWrapper _interOpWrapper;
    private readonly OtdrManager _otdrManager;

    public RtuManager(IOptions<MonitoringConfig> config, ILogger<RtuManager> logger,
        SerialPortManager serialPortManager, InterOpWrapper interOpWrapper, OtdrManager otdrManager)
    {
        _config = config;
        _logger = logger;
        _serialPortManager = serialPortManager;
        _interOpWrapper = interOpWrapper;
        _otdrManager = otdrManager;
    }

    public async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto? dto = null)
    {
        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
        var creationTime = File.GetLastWriteTime(assembly.Location);
        var version = $"{info.FileVersion}";

        var versionRtuManager = $"{info.FileVersion} built {creationTime:dd/MM/yyyy}";
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"RtuManager {versionRtuManager}");
        var res = _serialPortManager.ResetCharon();
        if (res != ReturnCode.Ok)
            return new RtuInitializedDto(res);

        _serialPortManager.ShowOnLedDisplay(LedDisplayCode.Connecting); // "Connecting..."

        var result = await _otdrManager.InitializeOtdr();
        if (result.ReturnCode != ReturnCode.Ok)
            return result;

        result.Version = version;
        result.Version2 = "";
          
        var result2 = await _otdrManager.InitializeOtau(result);
        if (!result2.IsInitialized)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "Failed initialize RTU!");
            return result2;
        }

        result2.IsMonitoringOn = _config.Value.IsMonitoringOn;

        result2.AcceptableMeasParams = _interOpWrapper.GetTreeOfAcceptableMeasParams();
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "RTU initialized successfully!" + Environment.NewLine);

        if (!_config.Value.IsMonitoringOn)
        {
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "RTU is in MANUAL mode, disconnect OTDR" + Environment.NewLine);
            var unused = await _otdrManager.DisconnectOtdr();
        }

        return result2;
    }

    public async Task<RequestAnswer> FreeOtdr()
    {
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "RtuManager: FreeOtdr");
        var res = await _otdrManager.DisconnectOtdr();
        return new RequestAnswer(res ? ReturnCode.Ok : ReturnCode.Error);
    }
}