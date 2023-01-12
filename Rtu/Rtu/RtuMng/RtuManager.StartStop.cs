using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    public async Task<RequestAnswer> ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
    {
        await Task.Delay(1);
        var wasMonitoringOn = _monitoringConfig.Value.IsMonitoringOn;
        if (_monitoringConfig.Value.IsMonitoringOn)
            StopMonitoring("Apply monitoring settings");

        SaveNewFrequenciesInConfig(dto.Timespans);
        _monitoringQueue.ComposeNewQueue(dto.Ports);
        _logger.LLog(Logs.RtuManager, $"Queue merged. {_monitoringQueue.Count()} port(s) in queue");
        _monitoringQueue.Save();
        _monitoringQueue.SaveBackup();

        if (dto.IsMonitoringOn)
            await StartMonitoring(wasMonitoringOn);
        return new RequestAnswer(ReturnCode.Ok);
    }

    private void SaveNewFrequenciesInConfig(MonitoringTimespansDto dto)
    {
        _monitoringConfig.Update(c => c.PreciseMakeTimespan = (int)dto.PreciseMeas.TotalSeconds);
        _preciseMakeTimespan = dto.PreciseMeas;
        _monitoringConfig.Update(c => c.PreciseSaveTimespan = (int)dto.PreciseSave.TotalSeconds);
        _preciseSaveTimespan = dto.PreciseSave;
        _monitoringConfig.Update(c => c.FastSaveTimespan = (int)dto.FastSave.TotalSeconds);
        _fastSaveTimespan = dto.FastSave;
    }

    private async Task StartMonitoring(bool wasMonitoringOn)
    {
        var rtuInitializationResult = await InitializeRtu();
        if (!rtuInitializationResult.IsInitialized)
        {
            while (await RunMainCharonRecovery() != ReturnCode.Ok) { }
        }

        if (!wasMonitoringOn)
            _monitoringQueue.RaiseMonitoringModeChangedFlag();

        _logger.LLog(Logs.RtuManager, Environment.NewLine + "RTU is turned into AUTOMATIC mode.");

        // с этого начинается цикл мониторинга
        // _monitoringConfig.Update(c=>c.LastMeasurementTimestamp = DateTime.Now.ToString(CultureInfo.CurrentCulture));
        // _monitoringConfig.Update(c=>c.IsMonitoringOn = true);

        RunMonitoringCycle();
    }

    public async Task<RequestAnswer> StopMonitoring()
    {
        await Task.Delay(1);
        StopMonitoring("Stop monitoring");
        return new RequestAnswer(ReturnCode.Ok);
    }

    private void StopMonitoring(string caller)
    {
        if (!_monitoringConfig.Value.IsMonitoringOn)
        {
            _logger.LLog(Logs.RtuManager, $"{caller}: RTU is in MANUAL mode already");
            return;
        }

        _monitoringConfig.Update(c => c.IsMonitoringOn = false);
        _logger.LLog(Logs.RtuManager, $"{caller}: Interrupting current measurement...");
        _cancellationTokenSource?.Cancel();

        // if Lmax = 240km and Time = 10min one step lasts 5-6 sec
        Thread.Sleep(TimeSpan.FromSeconds(6));
    }

}