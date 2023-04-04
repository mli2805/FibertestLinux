using System.Globalization;
using Fibertest.CharonLib;
using Fibertest.Dto;
using Fibertest.Utils;
using Fibertest.Utils.Recovery;

namespace Fibertest.Rtu
{
    public partial class RtuManager
    {
        public async Task<RequestAnswer> StartOutOfTurnMeasurement(DoOutOfTurnPreciseMeasurementDto dto)
        {
            await StopMonitoring("Out of turn precise measurement");
            if (!_wasMonitoringOn)
                await ConnectOtdrWithRecovering();
            var _ = Task.Run(() => DoOutOfTurn(dto));

            return new RequestAnswer(ReturnCode.Ok);
        }

        private async Task DoOutOfTurn(DoOutOfTurnPreciseMeasurementDto dto)
        {
            await Task.Delay(1);
        }

        public async Task<ClientMeasurementStartedDto> DoClientMeasurement(DoClientMeasurementDto dto)
        {
            if (!IsRtuInitialized)
            {
                _logger.Info(Logs.RtuService, "I am initializing now. Ignore command.");
                return new ClientMeasurementStartedDto(ReturnCode.RtuInitializationInProgress);
            }

            if (_config.Value.Monitoring.IsAutoBaseMeasurementInProgress)
            {
                _logger.Info(Logs.RtuService, "Auto Base Measurement In Progress. Ignore command.");
                return new ClientMeasurementStartedDto(ReturnCode.RtuAutoBaseMeasurementInProgress);
            }

            _logger.EmptyAndLog(Logs.RtuManager, "DoClientMeasurement command received");

            await StopMonitoring(dto.IsForAutoBase ? "Auto base measurement" : "Measurement (Client)");
            if (!_config.Value.Monitoring.KeepOtdrConnection)
            {
                if (!_wasMonitoringOn)
                    await ConnectOtdrWithRecovering();
            }

            _config.Update(c => c.Monitoring.KeepOtdrConnection = dto.KeepOtdrConnection);
            if (dto.IsForAutoBase)
            {
                _config.Update(c => c.Monitoring.IsAutoBaseMeasurementInProgress = true);
                _config.Update(c => c.Monitoring.LastMeasurementTimestamp =
                    DateTime.Now.ToString(CultureInfo.CurrentCulture));
            }

            _logger.Info(Logs.RtuService, "Start Measurement in another thread");
            // await Task.Factory.StartNew(() => { MeasureWrapped(dto); }); // blocking call
            var unused = Task.Run(() => { MeasureWrapped(dto); });
            _logger.Info(Logs.RtuService, "Measurement TASK started, return this fact to client");

            return new ClientMeasurementStartedDto(ReturnCode.MeasurementClientStartedSuccessfully)
            { ClientMeasurementId = Guid.NewGuid() };
        }

        private async void MeasureWrapped(DoClientMeasurementDto dto)
        {
            _logger.Debug(Logs.RtuManager, "Measurement client is in progress...");
            var result = await Measure(dto);
            _logger.Info(Logs.RtuManager, result.SorBytes != null
                ? $"Measurement Client done. Sor size is {result.SorBytes.Length}"
                : "Measurement (Client) failed");

            await _grpcR2DService.SendAnyR2DRequest<ClientMeasurementResultDto, RequestAnswer>(result);

            if (dto.IsForAutoBase)
            {
                _config.Update(c => c.Monitoring.IsAutoBaseMeasurementInProgress = false);
            }
            _logger.Info(Logs.RtuManager);

            if (_wasMonitoringOn)
            {
                _config.Value.Monitoring.IsMonitoringOn = true;
                _wasMonitoringOn = false;
                await RunMonitoringCycle();
            }
            else
            {
                if (!_config.Value.Monitoring.KeepOtdrConnection)
                {
                    _otdrManager.DisconnectOtdr();
                    _logger.Info(Logs.RtuManager, "RTU is in MANUAL mode.");
                }
            }
        }

        private async Task<ClientMeasurementResultDto> Measure(DoClientMeasurementDto dto)
        {
            ClientMeasurementResultDto result = new ClientMeasurementResultDto() { ClientConnectionId = dto.ClientConnectionId };
            var toggleResult = ToggleToPort2(dto.OtauPortDto[0]);
            if (toggleResult != CharonOperationResult.Ok)
                return result.Set(dto.OtauPortDto[0],
                    toggleResult == CharonOperationResult.MainOtauError
                        ? ReturnCode.RtuToggleToPortError : ReturnCode.RtuToggleToBopPortError);

            var prepareResult = dto.IsAutoLmax
                ? await PrepareAutoLmaxMeasurement(dto)
                : PrepareClientMeasurement(dto);

            if (prepareResult != ReturnCode.Ok)
                return result.Set(dto.OtauPortDto[0], prepareResult);

            return await ClientMeasurementItself(dto, dto.OtauPortDto[0]);
        }

        private async Task<ReturnCode> PrepareAutoLmaxMeasurement(DoClientMeasurementDto dto)
        {
            var lmax = _interOpWrapper.GetLinkCharacteristics(out ConnectionParams cp);
            if (lmax.Equals(-1.0))
            {
                _logger.Error(Logs.RtuManager, "Failed to get link characteristics");
                return ReturnCode.MeasurementPreparationError;
            }

            if (lmax.Equals(0) && cp.splice == 0)
            {
                if (await RunMainCharonRecovery() != ReturnCode.Ok)
                    await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                return ReturnCode.MeasurementPreparationError;
            }

            // temporary commented for experiments with old RTU
            // if (cp.snr_almax == 0)
            // {
            //     _rtuLog.AppendLine("SNR == 0, No fiber!");
            //     return ReturnCode.SnrIs0;
            // }
            var values = AutoBaseParams.GetPredefinedParamsForLmax(lmax, "IIT MAK-100");
            if (values == null)
            {
                _logger.Error(Logs.RtuManager, "Lmax is out of valid range");
                return ReturnCode.InvalidValueOfLmax;
            }

            var positions =
                _interOpWrapper.ValuesToPositions(dto.SelectedMeasParams!, values, _treeOfAcceptableMeasParams!);
            if (!_interOpWrapper.SetMeasParamsByPosition(positions!))
            {
                _logger.Error(Logs.RtuManager, "Failed to set measurement parameters");
                return ReturnCode.MeasurementPreparationError;
            }
            _logger.Info(Logs.RtuManager, "Auto measurement parameters applied");
            return ReturnCode.Ok;
        }

        private ReturnCode PrepareClientMeasurement(DoClientMeasurementDto dto)
        {
            _logger.Debug(Logs.RtuManager, "PrepareClientMeasurement ...");
            if (!_interOpWrapper.SetMeasParamsByPosition(dto.SelectedMeasParams!))
            {
                _logger.Error(Logs.RtuManager, "Failed to set measurement parameters");
                return ReturnCode.MeasurementPreparationError;
            }
            _logger.Info(Logs.RtuManager, "User's measurement parameters applied");
            return ReturnCode.Ok;
        }

        private async Task<ClientMeasurementResultDto> ClientMeasurementItself(DoClientMeasurementDto dto, OtauPortDto currentOtauPortDto)
        {
            var result = new ClientMeasurementResultDto() { ClientConnectionId = dto.ClientConnectionId };
            var activeBop = currentOtauPortDto.IsPortOnMainCharon
                ? null
                : new Charon(new NetAddress(currentOtauPortDto.NetAddress.Ip4Address, TcpPorts.IitBop),
                    false, _config.Value.Charon, _logger);

            _rtuManagerCts = new CancellationTokenSource();
            var tokens = new[] { _rtuManagerCts.Token, RtuServiceCancellationToken };

            var measResult = _otdrManager.DoManualMeasurement(tokens, true, activeBop);

            // during measurement or right now
            if (measResult == ReturnCode.MeasurementInterrupted)
            {
                _logger.Info(Logs.RtuManager, "Measurement (Client) interrupted.");
                _wasMonitoringOn = false;
                _config.Update(c => c.Monitoring.KeepOtdrConnection = false);
                return result.Set(currentOtauPortDto, ReturnCode.MeasurementInterrupted);
            }

            if (measResult != ReturnCode.MeasurementEndedNormally)
            {
                if (await RunMainCharonRecovery() != ReturnCode.Ok)
                    await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                return result.Set(currentOtauPortDto, ReturnCode.MeasurementError);
            }

            var lastSorDataBuffer = _otdrManager.GetLastSorDataBuffer();
            if (lastSorDataBuffer == null)
                return result.Set(currentOtauPortDto, ReturnCode.MeasurementError);

            return result.Set(currentOtauPortDto, ReturnCode.MeasurementEndedNormally,
               !dto.IsForAutoBase
                    ? _otdrManager.ApplyAutoAnalysis(lastSorDataBuffer)
                    : dto.IsInsertNewEvents
                        ? _otdrManager.Sf780_779(lastSorDataBuffer)
                        : _otdrManager.Sf780(lastSorDataBuffer));
        }

        private CharonOperationResult ToggleToPort2(OtauPortDto port)
        {
            var toggleResult = _mainCharon.SetExtendedActivePort(port.Serial!, port.OpticalPort);
            _logger.Info(Logs.RtuManager, toggleResult == CharonOperationResult.Ok
                ? "Toggled Ok."
                : toggleResult == CharonOperationResult.MainOtauError
                    ? "Failed to toggle to main otau port" : "Failed to toggle to additional otau port");

            if (toggleResult == CharonOperationResult.AdditionalOtauError)
            {
                var connectionTimeout = _config.Value.Charon.ConnectionTimeout;
                try
                {
                    MikrotikInBop.ConnectAndReboot(_logger, Logs.RtuManager.ToInt(), port.NetAddress.Ip4Address, connectionTimeout);
                }
                catch (Exception e)
                {
                    _logger.Error(Logs.RtuManager, $"Cannot connect Mikrotik {port.NetAddress.Ip4Address}" + e.Message);
                }
            }

            return toggleResult;
        }
    }
    public static class ClientMeasurementResultFactory
    {
        public static ClientMeasurementResultDto Set(this ClientMeasurementResultDto result,
            OtauPortDto otauPortDto, ReturnCode returnCode, byte[]? sorBytes = null)
        {
            result.ClientMeasurementId = Guid.NewGuid();
            result.ReturnCode = returnCode;
            result.OtauPortDto = otauPortDto;
            result.SorBytes = sorBytes;

            return result;
        }
    }
}
