﻿using System.Linq;
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
    public class OneVeexMeasurementExecutor : IOneMeasurementExecutor
    {
        private readonly ILogger _logger; 
        private readonly Model _readModel;
        private readonly GrpcC2RService _grpcC2RService;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly VeexMeasurementTool _veexMeasurementTool;
        private readonly LandmarksIntoBaseSetter _landmarksIntoBaseSetter;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;

        private Trace _trace = null!;
        public MeasurementModel Model { get; set; } = new MeasurementModel();

        public OneVeexMeasurementExecutor(IWritableConfig<ClientConfig> config, ILogger logger, CurrentUser currentUser, Model readModel,
            GrpcC2RService grpcC2RService, IDispatcherProvider dispatcherProvider,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            VeexMeasurementTool veexMeasurementTool,
            LandmarksIntoBaseSetter landmarksIntoBaseSetter, MeasurementAsBaseAssigner measurementAsBaseAssigner)
        {
            _logger = logger;
            _readModel = readModel;
            _grpcC2RService = grpcC2RService;
            _dispatcherProvider = dispatcherProvider;
            _veexMeasurementTool = veexMeasurementTool;
            _landmarksIntoBaseSetter = landmarksIntoBaseSetter;
            _measurementAsBaseAssigner = measurementAsBaseAssigner;

            Model.CurrentUser = currentUser;
            Model.MeasurementTimeout = config.Value.Miscellaneous.MeasurementTimeoutMs;
            Model.OtdrParametersTemplatesViewModel = new OtdrParametersTemplatesViewModel(config);
            Model.AutoAnalysisParamsViewModel = autoAnalysisParamsViewModel;
            Model.MeasurementProgressViewModel = new MeasurementProgressViewModel();
        }

        public bool Initialize(Rtu rtu, bool isForRtu)
        {
            Model.Rtu = rtu;

            Model.OtdrParametersTemplatesViewModel.Initialize(rtu, isForRtu);
            return Model.AutoAnalysisParamsViewModel.Initialize();
        }

        public async Task Start(TraceLeaf traceLeaf, bool keepOtdrConnection = false)
        {
            _trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            StartTimer();

            Model.MeasurementProgressViewModel.DisplayStartMeasurement(traceLeaf.Title);

            VeexMeasOtdrParameters? veexMeasOtdrParameters;
            if (Model.OtdrParametersTemplatesViewModel.IsAutoLmaxSelected())
            {
                var lineParamsDto = await _veexMeasurementTool.GetLineParametersAsync(Model, traceLeaf);
                if (lineParamsDto.ReturnCode != ReturnCode.Ok)
                {
                    MeasurementCompleted?
                        .Invoke(this, new MeasurementEventArgs(lineParamsDto.ReturnCode, _trace));
                    return;
                }

                veexMeasOtdrParameters = Model.OtdrParametersTemplatesViewModel.Model
                    .GetVeexMeasOtdrParametersBase(false)
                    .FillInWithTemplate(lineParamsDto.ConnectionQuality!, Model.Rtu.Omid!);

                if (veexMeasOtdrParameters == null)
                {
                    MeasurementCompleted?
                        .Invoke(this, new MeasurementEventArgs(ReturnCode.InvalidValueOfLmax, _trace));

                    Model.IsEnabled = true;             
                    return;
                }
            }
            else
            {
                veexMeasOtdrParameters = Model.OtdrParametersTemplatesViewModel.GetVeexSelectedParameters();
            }

            var dto = traceLeaf.Parent
                .CreateDoClientMeasurementDto(traceLeaf.PortNumber, false, _readModel, Model.CurrentUser)
                .SetParams(true, Model.AutoAnalysisParamsViewModel.SearchNewEvents, false, null, veexMeasOtdrParameters);

            var startResult =
                await _grpcC2RService.SendAnyC2RRequest<DoClientMeasurementDto, ClientMeasurementStartedDto>(dto);
            if (startResult.ReturnCode != ReturnCode.MeasurementClientStartedSuccessfully)
            {
                _timer.Stop();
                _timer.Dispose();
                Model.MeasurementProgressViewModel.DisplayStop();
                Model.IsEnabled = true;

                MeasurementCompleted?
                    .Invoke(this, new MeasurementEventArgs(startResult.ReturnCode, _trace, startResult.ErrorMessage ?? ""));

                Model.IsEnabled = true;
                return;
            }

            Model.MeasurementProgressViewModel.Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;

            var cts = new CancellationTokenSource();
            await Task.Delay(veexMeasOtdrParameters.averagingTime == @"00:05" ? 10000 : 20000);
            var veexResult = 
                await _veexMeasurementTool.Fetch(dto.RtuId, _trace, startResult.ClientMeasurementId, cts);
            if (veexResult.Code == ReturnCode.MeasurementEndedNormally)
            {
                var res = new ClientMeasurementResultDto(veexResult.Code) { SorBytes = veexResult.SorBytes };
                ProcessMeasurementResult(res);
            }
            else
            {
                _timer.Stop();
                _timer.Dispose();
                MeasurementCompleted?.Invoke(this, veexResult);
            }

        }

        private System.Timers.Timer _timer = null!;
        private void StartTimer()
        {
            _logger.Info(Logs.Client,@"Start a measurement timeout");
            _timer = new System.Timers.Timer(Model.MeasurementTimeout);
            _timer.Elapsed += TimeIsOver;
            _timer.AutoReset = false;
            _timer.Start();
        }
        private void TimeIsOver(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _logger.Info(Logs.Client,@"Measurement timeout expired");
            _timer.Dispose();

            _dispatcherProvider.GetDispatcher().Invoke(() =>
            {
                Model.MeasurementProgressViewModel.DisplayStop();

                MeasurementCompleted?
                    .Invoke(this, new MeasurementEventArgs(ReturnCode.MeasurementTimeoutExpired, _trace));
                Model.IsEnabled = true;
            });
        }

        public async void ProcessMeasurementResult(ClientMeasurementResultDto dto)
        {
            _timer.Stop();
            _timer.Dispose();

            if (dto.SorBytes == null)
            {
                MeasurementCompleted?
                    .Invoke(this, new MeasurementEventArgs(dto.ReturnCode, _trace));
                return;
            }

            _logger.Info(Logs.Client,@"Measurement (Client) result received");

            Model.MeasurementProgressViewModel.Message = Resources.SID_Applying_base_refs__Please_wait;

            var sorData = SorData.FromBytes(dto.SorBytes);
            var rftsParams = Model.AutoAnalysisParamsViewModel
                .GetRftsParams(sorData, Model.OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id, Model.Rtu);
            sorData.ApplyRftsParamsTemplate(rftsParams);

            _landmarksIntoBaseSetter.ApplyTraceToAutoBaseRef(sorData, _trace);
            _measurementAsBaseAssigner.Initialize(Model.Rtu);
            await Task.Delay(1000);
            var result = await _measurementAsBaseAssigner.Assign(sorData, _trace);

            MeasurementCompleted?
                .Invoke(this, result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully
                    ? new MeasurementEventArgs(ReturnCode.BaseRefAssignedSuccessfully, _trace, sorData.ToBytes())
                    : new MeasurementEventArgs(ReturnCode.BaseRefAssignmentFailed, _trace, result.ErrorMessage!));
        }

        public delegate void MeasurementHandler(object sender, MeasurementEventArgs e);

        public event OneIitMeasurementExecutor.MeasurementHandler? MeasurementCompleted;
    }

}