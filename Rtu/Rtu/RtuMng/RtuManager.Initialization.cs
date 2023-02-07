using Fibertest.Dto;
using Fibertest.Utils;
using System.Diagnostics;
using System.Reflection;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    public async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto? dto = null)
    {
        // prohibit to send heartbeats
        ShouldSendHeartbeat.TryDequeue(out _);

        if (dto != null)
            SaveInitializationParameters(dto);

        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
        var creationTime = File.GetLastWriteTime(assembly.Location);
        var version = $"{info.FileVersion}";

        var versionRtuManager = $"{info.FileVersion} built {creationTime:dd/MM/yyyy}";
        _logger.LogInfo(Logs.RtuManager, $"RtuManager {versionRtuManager}");
        var res = _serialPortManager.ResetCharon();
        if (res != ReturnCode.Ok)
            return new RtuInitializedDto(res);

        _serialPortManager.ShowOnLedDisplay(LedDisplayCode.Connecting); // "Connecting..."

        var result = await _otdrManager.InitializeOtdr();
        result.RtuId = _config.Value.General.RtuId;
        if (result.ReturnCode != ReturnCode.Ok)
            return result;

        result.Version = version;
        result.Version2 = "";

        var result2 = dto != null
            ? await ReInitializeOtauOnUsersRequest(dto, result)
            : await InitializeOtau(result); // on service or module restart
        if (!result2.IsInitialized)
        {
            _logger.LogError(Logs.RtuManager, "Failed initialize RTU!");
            return result2;
        }

        result2.IsMonitoringOn = _config.Value.Monitoring.IsMonitoringOn;

        _treeOfAcceptableMeasParams = _interOpWrapper.GetTreeOfAcceptableMeasParams();
        result2.AcceptableMeasParams = _treeOfAcceptableMeasParams;

        _logger.LogInfo(Logs.RtuManager, "RTU initialized successfully!");

        _monitoringQueue.Load();
        EvaluateFrequencies();

        _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.Ok);

        if (!_config.Value.Monitoring.IsMonitoringOn)
        {
            _logger.LogInfo(Logs.RtuManager, "RTU is in MANUAL mode, disconnect OTDR");
            var unused = await _otdrManager.DisconnectOtdr();
        }

        // permit to send heartbeats
        ShouldSendHeartbeat.Enqueue(new object());

        return result2;
    }

    public async Task<RequestAnswer> FreeOtdr()
    {
        _logger.LogInfo(Logs.RtuManager, "RtuManager: FreeOtdr");
        var res = await _otdrManager.DisconnectOtdr();
        return new RequestAnswer(res ? ReturnCode.Ok : ReturnCode.Error);
    }

    private void SaveInitializationParameters(InitializeRtuDto dto)
    {
        _config.Update(c => c.General.RtuId = dto.RtuId);
        _config.Update(c => c.General.ServerAddress = dto.ServerAddresses!);

        _config.Update(c => c.Monitoring.IsMonitoringOn = false);
        _logger.EmptyAndLog(Logs.RtuManager, "Initialization by the USER puts RTU into MANUAL mode.");
    }

    private void EvaluateFrequencies()
    {
        _preciseMakeTimespan = TimeSpan.FromSeconds(_config.Value.Monitoring.PreciseMakeTimespan);
        _preciseSaveTimespan = TimeSpan.FromSeconds(_config.Value.Monitoring.PreciseSaveTimespan);
        _fastSaveTimespan = TimeSpan.FromSeconds(_config.Value.Monitoring.FastSaveTimespan);
    }
}