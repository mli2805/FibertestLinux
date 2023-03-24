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

        if (IsMonitoringOn || _wasMonitoringOn)
        {
            _wasMonitoringOn = IsMonitoringOn;
            StopMonitoring("Initialize");
        }

        if (dto != null)
             SaveParametersAndStates(dto);

        IsAutoBaseMeasurementInProgress = false;
        _config.Update(c=>c.Monitoring.IsAutoBaseMeasurementInProgress = false);

        var version = LogInitializationStart();

        IsRtuInitialized = false;

        var res = _serialPortManager.ResetCharon();
        if (res != ReturnCode.Ok)
            return new RtuInitializedDto(res);

        _serialPortManager.ShowOnLedDisplay(LedDisplayCode.Connecting); // "Connecting..."

        var result = _otdrManager.InitializeOtdr();
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
            _logger.Error(Logs.RtuManager, "Failed initialize RTU!");
            return result2;
        }

        result2.IsMonitoringOn = _config.Value.Monitoring.IsMonitoringOn;

        _treeOfAcceptableMeasParams = _interOpWrapper.GetTreeOfAcceptableMeasParams();
        result2.AcceptableMeasParams = _treeOfAcceptableMeasParams;

        IsRtuInitialized = true;
        _logger.Info(Logs.RtuManager, "RTU initialized successfully!");

        await _monitoringQueue.Load();
        EvaluateFrequencies();

        _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.Ok);

        if (!_config.Value.Monitoring.IsMonitoringOn)
        {
            _logger.Info(Logs.RtuManager, "RTU is in MANUAL mode, disconnect OTDR");
            var unused = _otdrManager.DisconnectOtdr();
        }

        // permit to send heartbeats
        ShouldSendHeartbeat.Enqueue(new object());

        return result2;
    }

    private string LogInitializationStart()
    {
        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
        var creationTime = File.GetLastWriteTime(assembly.Location);
        var version = $"{info.FileVersion}";

        var versionRtuManager = $"{info.FileVersion} built {creationTime:dd/MM/yyyy}";
        _logger.EmptyAndLog(Logs.RtuManager, $"RtuManager {versionRtuManager}");

        return version;
    }

    public RequestAnswer FreeOtdr()
    {
        _logger.Info(Logs.RtuManager, "RtuManager: FreeOtdr");
        var res = _otdrManager.DisconnectOtdr();
        return new RequestAnswer(res ? ReturnCode.Ok : ReturnCode.Error);
    }

    private void SaveParametersAndStates(InitializeRtuDto dto)
    {
        _config.Update(c => c.General.RtuId = dto.RtuId);
        _config.Update(c => c.General.ServerAddress = dto.ServerAddresses!);

        IsMonitoringOn = false;
        _wasMonitoringOn = false;
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