using System.Globalization;
using Fibertest.CharonLib;
using Fibertest.Dto;
using Fibertest.Utils;
using Fibertest.Utils.Recovery;

namespace Fibertest.Rtu
{
    public partial class RtuManager
    {
        public async Task<ClientMeasurementStartedDto> DoClientMeasurement(DoClientMeasurementDto dto)
        {
            if (!IsRtuInitialized)
            {
                _logger.LogInfo(Logs.RtuService, "I am initializing now. Ignore command.");
                return new ClientMeasurementStartedDto(ReturnCode.RtuInitializationInProgress)
                { ClientMeasurementId = Guid.NewGuid() };
            }

            if (IsAutoBaseMeasurementInProgress)
            {
                _logger.LogInfo(Logs.RtuService, "Auto Base Measurement In Progress. Ignore command.");
                return new ClientMeasurementStartedDto(ReturnCode.RtuAutoBaseMeasurementInProgress)
                { ClientMeasurementId = Guid.NewGuid() };
            }

            _logger.EmptyAndLog(Logs.RtuManager, "DoClientMeasurement command received");

            if (!KeepOtdrConnection)
                StopMonitoringAndConnectOtdrWithRecovering(dto.IsForAutoBase ? "Auto base measurement" : "Measurement (Client)");

            KeepOtdrConnection = dto.KeepOtdrConnection;
            _config.Update(c => c.Monitoring.KeepOtdrConnection = KeepOtdrConnection);
            if (dto.IsForAutoBase)
            {
                IsAutoBaseMeasurementInProgress = true;
                _config.Update(c => c.Monitoring.IsAutoBaseMeasurementInProgress = true);
                _config.Update(c => c.Monitoring.LastMeasurementTimestamp =
                    DateTime.Now.ToString(CultureInfo.CurrentCulture));
            }

            _logger.LogInfo(Logs.RtuService, "Start Measurement in another thread");
            await Task.Factory.StartNew(() => { MeasureWrapped(dto); });
            _logger.LogInfo(Logs.RtuService, "Measurement TASK started, return this fact to client");

            return new ClientMeasurementStartedDto(ReturnCode.MeasurementClientStartedSuccessfully)
            { ClientMeasurementId = Guid.NewGuid() };
            // sends ClientMeasurementStartedDto (means "started successfully")
        }

        private async void MeasureWrapped(DoClientMeasurementDto dto)
        {
            _logger.LogDebug(Logs.RtuManager, "Measurement client is in progress...");
            var result = await Measure(dto);
            _logger.LogInfo(Logs.RtuManager, result.SorBytes != null
                ? $"Measurement Client done. Sor size is {result.SorBytes.Length}"
                : "Measurement (Client) failed");

            await _grpcR2DService.SendAnyR2DRequest<ClientMeasurementResultDto, RequestAnswer>(result);

            if (dto.IsForAutoBase)
            {
                IsAutoBaseMeasurementInProgress = false;
                _config.Update(c => c.Monitoring.IsAutoBaseMeasurementInProgress = false);
            }
            _logger.LogInfo(Logs.RtuManager);

            if (_wasMonitoringOn)
            {
                IsMonitoringOn = true;
                _wasMonitoringOn = false;
                RunMonitoringCycle();
            }
            else
            {
                if (!KeepOtdrConnection)
                {
                    _otdrManager.DisconnectOtdr();
                    _logger.LogInfo(Logs.RtuManager, "RTU is in MANUAL mode.");
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
                _logger.LogError(Logs.RtuManager,"Failed to get link characteristics");
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
                _logger.LogError(Logs.RtuManager,"Lmax is out of valid range");
                return ReturnCode.InvalidValueOfLmax;
            }

            var positions = 
                _interOpWrapper.ValuesToPositions(dto.SelectedMeasParams!, values, _treeOfAcceptableMeasParams!);
            if (!_interOpWrapper.SetMeasParamsByPosition(positions!))
            {
                _logger.LogError(Logs.RtuManager,"Failed to set measurement parameters");
                return ReturnCode.MeasurementPreparationError;
            }
            _logger.LogInfo(Logs.RtuManager,"Auto measurement parameters applied");
            return ReturnCode.Ok;
        }

        private ReturnCode PrepareClientMeasurement(DoClientMeasurementDto dto)
        {
            _logger.LogDebug(Logs.RtuManager,"PrepareClientMeasurement ...");
            if (!_interOpWrapper.SetMeasParamsByPosition(dto.SelectedMeasParams!))
            {
                _logger.LogError(Logs.RtuManager,"Failed to set measurement parameters");
                return ReturnCode.MeasurementPreparationError;
            }
            _logger.LogInfo(Logs.RtuManager,"User's measurement parameters applied");
            return ReturnCode.Ok;
        }

        private async Task<ClientMeasurementResultDto> ClientMeasurementItself(DoClientMeasurementDto dto, OtauPortDto currentOtauPortDto)
        {
            var result = new ClientMeasurementResultDto() { ClientConnectionId = dto.ClientConnectionId };
            var activeBop = currentOtauPortDto.IsPortOnMainCharon
                ? null
                : new Charon(new NetAddress(currentOtauPortDto.NetAddress.Ip4Address, TcpPorts.IitBop),
                    false, _config.Value.Charon, _logger);
            _cancellationTokenSource = new CancellationTokenSource();
            var measResult = _otdrManager.DoManualMeasurement(_cancellationTokenSource, true, activeBop);

            // во время измерения или прямо сейчас
            if (measResult == ReturnCode.MeasurementInterrupted || _cancellationTokenSource.IsCancellationRequested)
            {
                _logger.LogInfo(Logs.RtuManager,"Measurement (Client) interrupted.");
                _wasMonitoringOn = false;
                KeepOtdrConnection = false;
                _config.Update(c=>c.Monitoring.KeepOtdrConnection = KeepOtdrConnection);
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
            _logger.LogInfo(Logs.RtuManager, toggleResult == CharonOperationResult.Ok
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
                    _logger.LogError(Logs.RtuManager, $"Cannot connect Mikrotik {port.NetAddress.Ip4Address}" + e.Message);
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
