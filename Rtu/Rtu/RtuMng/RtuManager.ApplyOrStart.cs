﻿using System.Diagnostics;
using System.Globalization;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    public async Task<RequestAnswer> ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;
        _logger.Info(Logs.RtuManager, $"gRPC command received in process {pid}, thread {tid}");

        var wasMonitoringOn = _config.Value.Monitoring.IsMonitoringOn;
        if (_config.Value.Monitoring.IsMonitoringOn)
            await StopMonitoring("Apply monitoring settings");

        SaveNewFrequenciesInConfig(dto.Timespans);
        _monitoringQueue.ComposeNewQueue(dto.Ports);
        _logger.Info(Logs.RtuManager, $"Queue merged. {_monitoringQueue.Count()} port(s) in queue");
        await _monitoringQueue.Save();
        await _monitoringQueue.SaveBackup();

        if (dto.IsMonitoringOn)
            await StartMonitoring(wasMonitoringOn);
        _logger.Debug(Logs.RtuManager, $"Leaving gRPC thread: process {pid}, thread {tid}");
        return new RequestAnswer(ReturnCode.MonitoringSettingsAppliedSuccessfully);
    }

    private void SaveNewFrequenciesInConfig(MonitoringTimespansDto dto)
    {
        _config.Update(c => c.Monitoring.PreciseMakeTimespan = (int)dto.PreciseMeas.TotalSeconds);
        _preciseMakeTimespan = dto.PreciseMeas;
        _config.Update(c => c.Monitoring.PreciseSaveTimespan = (int)dto.PreciseSave.TotalSeconds);
        _preciseSaveTimespan = dto.PreciseSave;
        _config.Update(c => c.Monitoring.FastSaveTimespan = (int)dto.FastSave.TotalSeconds);
        _fastSaveTimespan = dto.FastSave;
    }

    private async Task StartMonitoring(bool wasMonitoringOn)
    {
        var rtuInitializationResult = await InitializeRtu(); // will corrupt IsMonitoringOn
        if (!rtuInitializationResult.IsInitialized)
        {
            while (await RunMainCharonRecovery() != ReturnCode.Ok) { }
        }
        _config.Update(c => c.Monitoring.IsMonitoringOn = true);

        if (!wasMonitoringOn)
            _monitoringQueue.RaiseMonitoringModeChangedFlag();

        _logger.EmptyAndLog(Logs.RtuManager, "RTU is turned into AUTOMATIC mode.");

        _config.Update(c => c.Monitoring.LastMeasurementTimestamp = DateTime.Now.ToString(CultureInfo.CurrentCulture));

        var _ = Task.Run(RunMonitoringCycle);
    }
}