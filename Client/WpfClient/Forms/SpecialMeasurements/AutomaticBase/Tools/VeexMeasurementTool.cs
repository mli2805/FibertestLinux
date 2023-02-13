using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class VeexMeasurementTool
    {
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly ILogger _logger; 
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly GrpcC2RService _grpcC2RService;

        public VeexMeasurementTool(IWritableConfig<ClientConfig> config, ILogger logger, 
            Model readModel, IWcfServiceCommonC2D c2DWcfCommonManager,  GrpcC2RService grpcC2RService)
        {
            _config = config;
            _logger = logger;
            _readModel = readModel;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _grpcC2RService = grpcC2RService;
        }

        public async Task<LineParametersDto> GetLineParametersAsync(MeasurementModel model, TraceLeaf traceLeaf)
        {
            var templateModel = model.OtdrParametersTemplatesViewModel.Model;
            var veexMeasOtdrParameters = templateModel.GetVeexMeasOtdrParametersBase(true);
            var dto = traceLeaf.Parent
                .CreateDoClientMeasurementDto(traceLeaf.PortNumber, false, _readModel, model.CurrentUser)
                .SetParams(true, model.AutoAnalysisParamsViewModel.SearchNewEvents,true, null, veexMeasOtdrParameters);

            // with dto.VeexMeasOtdrParameters.measurementType == "auto_skip_measurement" - it is request of line quality
            var startResult =
                await _grpcC2RService.SendAnyC2RRequest<DoClientMeasurementDto, ClientMeasurementStartedDto>(dto);
            if (startResult.ReturnCode != ReturnCode.MeasurementClientStartedSuccessfully)
                return new LineParametersDto(){ReturnCode = startResult.ReturnCode};

            ClientMeasurementVeexResultDto? lineCheckResult = null;
            var p = _config.Value.Miscellaneous.VeexLineParamsTimeoutMs;
            bool flag = false;
            while (!flag)
            {
                await Task.Delay(p);
                var getDto = new GetClientMeasurementDto(dto.RtuId, RtuMaker.VeEX)
                {
                    VeexMeasurementId = startResult.ClientMeasurementId.ToString(),
                };  
                lineCheckResult = await _c2DWcfCommonManager.GetClientMeasurementAsync(getDto);
                _logger.LogInfo(Logs.Client,$@"lineCheckResult: {lineCheckResult.ReturnCode}");
                if (lineCheckResult.ReturnCode != ReturnCode.Ok)
                {
                    _logger.LogError(Logs.Client,@"Failed to get line parameters");
                    return new LineParametersDto(){ReturnCode = lineCheckResult.ReturnCode};
                }

                flag = lineCheckResult.VeexMeasurementStatus == @"finished";
            }

            var cq = lineCheckResult!.ConnectionQuality[0];
            _logger.LogInfo(Logs.Client,$@"lmax = {cq.lmaxKm:F},  loss = {cq.loss:F},  reflectance = {cq.reflectance:F},  SNR = {cq.snr:F}");
            return new LineParametersDto()
                { ReturnCode = ReturnCode.Ok, ConnectionQuality = lineCheckResult.ConnectionQuality[0] };
        }
        
        public async Task<MeasurementEventArgs> Fetch(Guid rtuId, Trace? trace, 
            Guid clientMeasurementId, CancellationTokenSource cts)
        {
            var getDto = new GetClientMeasurementDto(rtuId, RtuMaker.VeEX)
            {
                VeexMeasurementId = clientMeasurementId.ToString(),
            };
            while (!cts.IsCancellationRequested)
            {
                var measResult = await _c2DWcfCommonManager.GetClientMeasurementAsync(getDto);

                if (measResult.ReturnCode != ReturnCode.Ok || measResult.VeexMeasurementStatus == @"failed")
                {
                    var firstLine = measResult.ReturnCode != ReturnCode.Ok
                        ? measResult.ReturnCode.GetLocalizedString()
                        : Resources.SID_Failed_to_do_Measurement_Client__;

                    return new MeasurementEventArgs(
                        ReturnCode.FetchMeasurementFromRtu4000Failed,
                        trace,
                        new List<string>() 
                                {
                                    firstLine,
                                    "",
                                    measResult.ErrorMessage ?? "",
                                });
                }

                if (measResult.ReturnCode == ReturnCode.Ok && measResult.VeexMeasurementStatus == @"finished")
                {
                    var measResultWithSorBytes = await _c2DWcfCommonManager.GetClientMeasurementSorBytesAsync(getDto);
                    _logger.LogInfo(Logs.Client,$@"Fetched measurement {clientMeasurementId.First6()} from VEEX RTU");
                    return new MeasurementEventArgs(
                        ReturnCode.MeasurementEndedNormally, trace, measResultWithSorBytes.SorBytes);
                }

                await Task.Delay(2000);
            }

            _logger.LogInfo(Logs.Client,@"cancellation token received, stop fetching");
            return new MeasurementEventArgs(ReturnCode.MeasurementInterrupted, trace, new List<string>());
        }

    }
}
