using System.Globalization;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    private async void RunMonitoringCycle()
    {
        _monitoringConfig.Update(c => c.LastMeasurementTimestamp = DateTime.Now.ToString(CultureInfo.CurrentCulture));
        _monitoringConfig.Update(c => c.IsMonitoringOn = true);
        _logger.LogInfo(Logs.RtuManager, Environment.NewLine + "Start monitoring.");
        _logger.LogInfo(Logs.RtuManager, $"_mainCharon.Serial = {_mainCharon.Serial}");

        if (_monitoringQueue.Count() < 1)
        {
            _logger.LogInfo(Logs.RtuManager, "There are no ports in queue for monitoring.");
            _monitoringConfig.Update(c => c.IsMonitoringOn = false);
            return;
        }

        while (true)
        {
            _measurementNumber++;
            var monitoringPort = _monitoringQueue.Peek();

            await ProcessOnePort(monitoringPort);

            if (monitoringPort.LastMoniResult!.MeasurementResult != MeasurementResult.Interrupted)
            {
                var unused = _monitoringQueue.Dequeue();
                _monitoringQueue.Enqueue(monitoringPort);
            }


            if (!_monitoringConfig.Value.IsMonitoringOn)
                break;
        }

        _logger.LogInfo(Logs.RtuManager, "Monitoring stopped.");

        _monitoringConfig.Update(c => c.IsMonitoringOn = false);
        await _otdrManager.DisconnectOtdr();
        _logger.LogInfo(Logs.RtuManager, "Rtu is turned into MANUAL mode.");
    }

    private async Task ProcessOnePort(MonitoringPort monitoringPort)
    {
        var hasFastPerformed = false;

        var isNewTrace = monitoringPort.LastTraceState == FiberState.Unknown;
        // FAST 
        if ((_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastFastSavedTimestamp > _fastSaveTimespan) ||
            (monitoringPort.LastTraceState == FiberState.Ok || isNewTrace))
        {
            monitoringPort.LastMoniResult = await DoFastMeasurement(monitoringPort);
            if (monitoringPort.LastMoniResult.MeasurementResult != MeasurementResult.Success)
                return;
            hasFastPerformed = true;
        }

        var isTraceBroken = monitoringPort.LastTraceState != FiberState.Ok;
        var isSecondMeasurementNeeded =
            isNewTrace ||
            isTraceBroken ||
            _preciseMakeTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastPreciseMadeTimestamp > _preciseMakeTimespan;

        if (isSecondMeasurementNeeded)
        {
            // PRECISE (or ADDITIONAL)
            var baseType = (isTraceBroken && monitoringPort.IsBreakdownCloserThen20Km &&
                            monitoringPort.HasAdditionalBase())
                ? BaseRefType.Additional
                : BaseRefType.Precise;

            monitoringPort.LastMoniResult = await DoSecondMeasurement(monitoringPort, hasFastPerformed, baseType);
        }

        monitoringPort.IsMonitoringModeChanged = false;
        monitoringPort.IsConfirmationRequired = false;
    }

    private async Task<MoniResult> DoFastMeasurement(MonitoringPort monitoringPort)
    {
        _logger.LogInfo(Logs.RtuManager,
            Environment.NewLine + $"MEAS. {_measurementNumber}, Fast, port {monitoringPort.ToStringB(_mainCharon)}");

        var moniResult = await DoMeasurement(BaseRefType.Fast, monitoringPort);

        if (moniResult.MeasurementResult == MeasurementResult.Success)
        {
            if (moniResult.GetAggregatedResult() != FiberState.Ok)
                monitoringPort.IsBreakdownCloserThen20Km = moniResult.FirstBreakDistance < 20;

            var message = "";

            if (monitoringPort.LastTraceState == FiberState.Unknown) // 740)
            {
                message = "First measurement on port";
            }
            else if (moniResult.GetAggregatedResult() != monitoringPort.LastTraceState)
            {
                message = $"Trace state has changed ({monitoringPort.LastTraceState} => {moniResult.GetAggregatedResult()})";
                monitoringPort.IsConfirmationRequired = true;
            }
            else if (_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastFastSavedTimestamp > _fastSaveTimespan)
            {
                _logger.LogInfo(Logs.RtuManager,
                    $"last fast saved - {monitoringPort.LastFastSavedTimestamp}, _fastSaveTimespan - {_fastSaveTimespan.TotalMinutes} minutes");
                message = "It's time to save fast reflectogram";
            }
            monitoringPort.LastMoniResult = moniResult;
            monitoringPort.LastTraceState = moniResult.GetAggregatedResult();

            if (message != "")
            {
                _logger.LogInfo(Logs.RtuManager, "Send by MSMQ:  " + message);
                SendByMsmq(CreateDto(moniResult, monitoringPort));
                monitoringPort.LastFastSavedTimestamp = DateTime.Now;
            }

            _monitoringQueue.Save();
        }
        return moniResult;
    }

    private async Task<MoniResult> DoSecondMeasurement(MonitoringPort monitoringPort, bool hasFastPerformed,
        BaseRefType baseType, bool isOutOfTurnMeasurement = false)
    {
        _logger.LogInfo(Logs.RtuManager,
            Environment.NewLine + $"MEAS. {_measurementNumber}, {baseType}, port {monitoringPort.ToStringB(_mainCharon)}");

        var moniResult = await DoMeasurement(baseType, monitoringPort, !hasFastPerformed);

        if (moniResult.MeasurementResult == MeasurementResult.Success)
        {
            var message = "";
            if (isOutOfTurnMeasurement)
                message = "It's out of turn precise measurement";
            else if (moniResult.GetAggregatedResult() != monitoringPort.LastTraceState)
            {
                message = $"Trace state has changed ({monitoringPort.LastTraceState} => {moniResult.GetAggregatedResult()})";
            }
            // else if (monitoringPort.IsMonitoringModeChanged)
            //     message = "Monitoring mode was changed";
            else if (monitoringPort.IsConfirmationRequired)
                message = "Accident confirmation - should be saved";
            else if (_preciseSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastPreciseSavedTimestamp > _preciseSaveTimespan)
                message = "It's time to save precise reflectogram";

            monitoringPort.LastPreciseMadeTimestamp = DateTime.Now;
            monitoringPort.LastMoniResult = moniResult;
            monitoringPort.LastTraceState = moniResult.GetAggregatedResult();

            if (message != "")
            {
                _logger.LogInfo(Logs.RtuManager, "Send by MSMQ:  " + message);
                SendByMsmq(CreateDto(moniResult, monitoringPort));
                monitoringPort.LastPreciseSavedTimestamp = DateTime.Now;
            }

            _monitoringQueue.Save();
        }
        return moniResult;
    }

    private async Task<MoniResult> DoMeasurement(BaseRefType baseRefType, MonitoringPort monitoringPort, bool shouldChangePort = true)
    {
        _cancellationTokenSource = new CancellationTokenSource();

        if (shouldChangePort && !await ToggleToPort(monitoringPort))
            return new MoniResult() { MeasurementResult = MeasurementResult.ToggleToPortFailed };

        var baseBytes = monitoringPort.GetBaseBytes(baseRefType, _logger);
        if (baseBytes == null)
            return new MoniResult() { MeasurementResult = baseRefType.ToMeasurementResultProblem() };

        SendCurrentMonitoringStep(MonitoringCurrentStep.Measure, monitoringPort, baseRefType);

        _monitoringConfig.Update(c => c.LastMeasurementTimestamp = DateTime.Now.ToString(CultureInfo.CurrentCulture));

        if (_cancellationTokenSource.IsCancellationRequested) // command to interrupt monitoring came while port toggling
            return new MoniResult() { MeasurementResult = MeasurementResult.Interrupted };

        var result = _otdrManager.MeasureWithBase(_cancellationTokenSource, baseBytes, _mainCharon.GetActiveChildCharon());

        if (result == ReturnCode.MeasurementInterrupted)
        {
            _monitoringConfig.Update(c => c.IsMonitoringOn = false);
            SendCurrentMonitoringStep(MonitoringCurrentStep.Interrupted);
            return new MoniResult() { MeasurementResult = MeasurementResult.Interrupted };
        }

        if (result != ReturnCode.MeasurementEndedNormally)
        {
            if (await RunMainCharonRecovery() != ReturnCode.Ok)
                await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            return new MoniResult() { MeasurementResult = MeasurementResult.HardwareProblem };
        }

        _recoveryConfig.Update(c => c.RecoveryStep = RecoveryStep.Ok);

        SendCurrentMonitoringStep(MonitoringCurrentStep.Analysis, monitoringPort, baseRefType);
        var buffer = _otdrManager.GetLastSorDataBuffer();
        if (buffer == null)
            return new MoniResult() { MeasurementResult = MeasurementResult.FailedGetSorBuffer };
        if (_monitoringConfig.Value.ShouldSaveSorData)
            monitoringPort.SaveSorData(baseRefType, buffer, SorType.Raw, _logger); // for investigations purpose
        monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Raw, _logger); // for investigations purpose
        _logger.LogInfo(Logs.RtuManager, $"Measurement result ({buffer.Length} bytes).");

        try
        {
            // sometimes GetLastSorDataBuffer returns not full sor data, so
            // just to check whether OTDR still works and measurement is reliable
            if (!_interOpWrapper.PrepareMeasurement(true))
            {
                _logger.LogInfo(Logs.RtuManager, "Additional check after measurement failed!");
                monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Error, _logger); // save meas if error
                await ReInitializeDlls();
                return new MoniResult() { MeasurementResult = MeasurementResult.HardwareProblem };
            }
        }
        catch (Exception e)
        {
            _logger.LogInfo(Logs.RtuManager, $"Exception during PrepareMeasurement: {e.Message}");
        }

        MoniResult moniResult;
        try
        {
            _logger.LogInfo(Logs.RtuManager, "Start auto analysis.");
            var measBytes = _otdrManager.ApplyAutoAnalysis(buffer);
            if (measBytes == null)
                return new MoniResult() { MeasurementResult = MeasurementResult.FailedGetSorBuffer }; if (_monitoringConfig.Value.ShouldSaveSorData)
                monitoringPort.SaveSorData(baseRefType, buffer, SorType.Analysis, _logger); // for investigations purpose
            monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Analysis, _logger); // 
            _logger.LogInfo(Logs.RtuManager, $"Auto analysis applied. Now sor data has {measBytes.Length} bytes.");
            moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true); // base is inserted into meas during comparison
            if (_monitoringConfig.Value.ShouldSaveSorData)
                monitoringPort.SaveSorData(baseRefType, buffer, SorType.Meas, _logger); // for investigations purpose
            monitoringPort.SaveMeasBytes(baseRefType, measBytes, SorType.Meas, _logger); // so re-save meas after comparison
            moniResult.BaseRefType = baseRefType;
        }
        catch (Exception e)
        {
            _logger.LogInfo(Logs.RtuManager, $"Exception during measurement analysis: {e.Message}");
            return new MoniResult() { MeasurementResult = MeasurementResult.ComparisonFailed };
        }

        _logger.LogInfo(Logs.RtuManager, $"Trace state is {moniResult.GetAggregatedResult()}");
        foreach (var accidentInSor in moniResult.Accidents)
            _logger.LogInfo(Logs.RtuManager, accidentInSor.ToString());
        return moniResult;
    }

    private async Task ReInitializeDlls()
    {
        await _otdrManager.DisconnectOtdr();
        var otdrInitializationResult = _otdrManager.InitializeOtdr();
        _logger.LogInfo(Logs.RtuManager, Environment.NewLine + $"OTDR initialization result - {otdrInitializationResult}");
        _logger.LogInfo(Logs.RtuService, Environment.NewLine + $"OTDR initialization result - {otdrInitializationResult}");
    }


    private MonitoringResultDto CreateDto(MoniResult moniResult, MonitoringPort monitoringPort)
    {
        var otauPortDto = new OtauPortDto(monitoringPort.OpticalPort, monitoringPort.IsPortOnMainCharon, monitoringPort.CharonSerial);
        var portWithTraceDto = new PortWithTraceDto(otauPortDto, monitoringPort.TraceId);
        var dto = new MonitoringResultDto(
                _id, DateTime.Now, portWithTraceDto, moniResult.BaseRefType, moniResult.GetAggregatedResult())
        { SorBytes = moniResult.SorBytes };
        return dto;
    }

    private void SendByMsmq(MonitoringResultDto dto)
    {
        _logger.LogError(Logs.RtuManager, dto.RtuId.First6());
    }

    private void SendByMsmq(BopStateChangedDto dto)
    {
        _logger.LogError(Logs.RtuManager, dto.RtuId.First6());
    }
}