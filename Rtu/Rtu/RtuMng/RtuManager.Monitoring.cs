using System.Globalization;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    public async Task RunMonitoringCycle()
    {
        _config.Update(c => c.Monitoring.LastMeasurementTimestamp = DateTime.Now.ToString(CultureInfo.CurrentCulture));
        _config.Update(c => c.Monitoring.IsMonitoringOn = true);
        _logger.EmptyAndLog(Logs.RtuManager, "Start monitoring.");
        _logger.Info(Logs.RtuManager, $"_mainCharon.Serial = {_mainCharon.Serial}");

        if (_monitoringQueue.Count() < 1)
        {
            _logger.Info(Logs.RtuManager, "There are no ports in queue for monitoring.");
            _config.Update(c => c.Monitoring.IsMonitoringOn = false);
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


            if (!_config.Value.Monitoring.IsMonitoringOn)
                break;
        }

        _logger.Info(Logs.RtuManager, "Monitoring stopped.");

        _config.Update(c => c.Monitoring.IsMonitoringOn = false);
        _otdrManager.DisconnectOtdr();
        _logger.Info(Logs.RtuManager, "Rtu is turned into MANUAL mode.");
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
        _logger.Info(Logs.RtuManager,
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
                _logger.Info(Logs.RtuManager,
                    $"last fast saved - {monitoringPort.LastFastSavedTimestamp}, _fastSaveTimespan - {_fastSaveTimespan.TotalMinutes} minutes");
                message = "It's time to save fast reflectogram";
            }
            monitoringPort.LastMoniResult = moniResult;
            monitoringPort.LastTraceState = moniResult.GetAggregatedResult();

            if (message != "")
            {
                _logger.Info(Logs.RtuManager, "Send by MSMQ:  " + message);
                SendByMsmq(CreateDto(moniResult, monitoringPort));
                monitoringPort.LastFastSavedTimestamp = DateTime.Now;
            }

            await _monitoringQueue.Save();
        }
        return moniResult;
    }

    private async Task<MoniResult> DoSecondMeasurement(MonitoringPort monitoringPort, bool hasFastPerformed,
        BaseRefType baseType, bool isOutOfTurnMeasurement = false)
    {
        _logger.Info(Logs.RtuManager,
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
                _logger.Info(Logs.RtuManager, "Send by MSMQ:  " + message);
                SendByMsmq(CreateDto(moniResult, monitoringPort));
                monitoringPort.LastPreciseSavedTimestamp = DateTime.Now;
            }

            await _monitoringQueue.Save();
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

        _config.Update(c => c.Monitoring.LastMeasurementTimestamp = DateTime.Now.ToString(CultureInfo.CurrentCulture));

        if (_cancellationTokenSource.IsCancellationRequested) // command to interrupt monitoring came while port toggling
            return new MoniResult() { MeasurementResult = MeasurementResult.Interrupted };

        var result = _otdrManager.MeasureWithBase(_cancellationTokenSource, baseBytes, _mainCharon.GetActiveChildCharon());

        if (result == ReturnCode.MeasurementInterrupted)
        {
            _config.Update(c => c.Monitoring.IsMonitoringOn = false);
            SendCurrentMonitoringStep(MonitoringCurrentStep.Interrupted);
            return new MoniResult() { MeasurementResult = MeasurementResult.Interrupted };
        }

        if (result != ReturnCode.MeasurementEndedNormally)
        {
            if (await RunMainCharonRecovery() != ReturnCode.Ok)
                await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            return new MoniResult() { MeasurementResult = MeasurementResult.HardwareProblem };
        }

        _config.Update(c => c.Recovery.RecoveryStep = RecoveryStep.Ok);

        SendCurrentMonitoringStep(MonitoringCurrentStep.Analysis, monitoringPort, baseRefType);
        var buffer = _otdrManager.GetLastSorDataBuffer();
        if (buffer == null)
            return new MoniResult() { MeasurementResult = MeasurementResult.FailedGetSorBuffer };
        if (_config.Value.Monitoring.ShouldSaveSorData)
            monitoringPort.SaveSorData(baseRefType, buffer, SorType.Raw, _logger); // for investigations purpose
        monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Raw, _logger); // for investigations purpose
        _logger.Info(Logs.RtuManager, $"Measurement result ({buffer.Length} bytes).");

        try
        {
            // sometimes GetLastSorDataBuffer returns not full sor data, so
            // just to check whether OTDR still works and measurement is reliable
            if (!_interOpWrapper.PrepareMeasurement(true))
            {
                _logger.Info(Logs.RtuManager, "Additional check after measurement failed!");
                monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Error, _logger); // save meas if error
                ReInitializeDlls();
                return new MoniResult() { MeasurementResult = MeasurementResult.HardwareProblem };
            }
        }
        catch (Exception e)
        {
            _logger.Info(Logs.RtuManager, $"Exception during PrepareMeasurement: {e.Message}");
        }

        MoniResult moniResult;
        try
        {
            _logger.Info(Logs.RtuManager, "Start auto analysis.");
            var measBytes = _otdrManager.ApplyAutoAnalysis(buffer);
            if (measBytes == null)
                return new MoniResult() { MeasurementResult = MeasurementResult.FailedGetSorBuffer }; 
            if (_config.Value.Monitoring.ShouldSaveSorData)
                monitoringPort.SaveSorData(baseRefType, buffer, SorType.Analysis, _logger); // for investigations purpose
            monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Analysis, _logger); // 
            _logger.Info(Logs.RtuManager, $"Auto analysis applied. Now sor data has {measBytes.Length} bytes.");
            moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true); // base is inserted into meas during comparison
            if (_config.Value.Monitoring.ShouldSaveSorData)
                monitoringPort.SaveSorData(baseRefType, buffer, SorType.Meas, _logger); // for investigations purpose
            monitoringPort.SaveMeasBytes(baseRefType, measBytes, SorType.Meas, _logger); // so re-save meas after comparison
            moniResult.BaseRefType = baseRefType;
        }
        catch (Exception e)
        {
            _logger.Info(Logs.RtuManager, $"Exception during measurement analysis: {e.Message}");
            return new MoniResult() { MeasurementResult = MeasurementResult.ComparisonFailed };
        }

        _logger.Info(Logs.RtuManager, $"Trace state is {moniResult.GetAggregatedResult()}");
        foreach (var accidentInSor in moniResult.Accidents)
            _logger.Info(Logs.RtuManager, accidentInSor.ToString());
        return moniResult;
    }

    private void ReInitializeDlls()
    {
        _otdrManager.DisconnectOtdr();
        var otdrInitializationResult = _otdrManager.InitializeOtdr();
        _logger.Info(Logs.RtuManager, Environment.NewLine + $"OTDR initialization result - {otdrInitializationResult}");
        _logger.Info(Logs.RtuService, Environment.NewLine + $"OTDR initialization result - {otdrInitializationResult}");
    }


    private MonitoringResultDto CreateDto(MoniResult moniResult, MonitoringPort monitoringPort)
    {
        var otauPortDto = new OtauPortDto(monitoringPort.OpticalPort, monitoringPort.IsPortOnMainCharon)
        {
            Serial =  monitoringPort.CharonSerial
        };
        var portWithTraceDto = new PortWithTraceDto(otauPortDto, monitoringPort.TraceId);
        var dto = new MonitoringResultDto(
                _id, DateTime.Now, portWithTraceDto, moniResult.BaseRefType, moniResult.GetAggregatedResult())
        { SorBytes = moniResult.SorBytes };
        return dto;
    }

    private void SendByMsmq(MonitoringResultDto dto)
    {
        _logger.Error(Logs.RtuManager, dto.RtuId.First6());
    }

    private void SendByMsmq(BopStateChangedDto dto)
    {
        _logger.Error(Logs.RtuManager, dto.RtuId.First6());
    }
}