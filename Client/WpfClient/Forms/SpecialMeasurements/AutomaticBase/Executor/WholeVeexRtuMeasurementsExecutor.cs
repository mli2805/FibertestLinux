using System.Threading;
using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using GrpsClientLib;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class WholeVeexRtuMeasurementsExecutor : IWholeRtuMeasurementsExecutor
    {
        private readonly ILogger _logger; 
        private readonly Model _readModel;
        private readonly GrpcC2RService _grpcC2RService;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly VeexMeasurementTool _veexMeasurementTool;
        private readonly LandmarksIntoBaseSetter _landmarksIntoBaseSetter;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;

        private Trace _trace;

        public MeasurementModel Model { get; set; } = new MeasurementModel();

        public WholeVeexRtuMeasurementsExecutor(IWritableConfig<ClientConfig> config, ILogger logger, CurrentUser currentUser, Model readModel,
            GrpcC2RService grpcC2RService, IDispatcherProvider dispatcherProvider,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            VeexMeasurementTool veexMeasurementTool,
            LandmarksIntoBaseSetter landmarksIntoBaseSetter, MeasurementAsBaseAssigner measurementAsBaseAssigner
        )
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

        public bool Initialize(Rtu rtu)
        {
            Model.Rtu = rtu;
            Model.TraceResults.Clear();
            Model.InterruptedPressed = false;

            Model.OtdrParametersTemplatesViewModel.Initialize(rtu, true);
            return Model.AutoAnalysisParamsViewModel.Initialize();
        }

        private CancellationTokenSource _cts;
        public async Task StartOneMeasurement(RtuAutoBaseProgress item, bool keepOtdrConnection = false)
        {
            _logger.EmptyAndLog(Logs.Client,$@"Start auto base measurement for {item.Trace.Title}.");
            _trace = item.Trace;
            StartTimer();

            Model.MeasurementProgressViewModel.DisplayStartMeasurement(item.Trace.Title);

            var lineParamsDto = await _veexMeasurementTool.GetLineParametersAsync(Model, item.TraceLeaf);
            if (lineParamsDto.ReturnCode != ReturnCode.Ok)
            {
                MeasurementCompleted?
                    .Invoke(this, new MeasurementEventArgs(lineParamsDto.ReturnCode, _trace));
                return;
            }

            var veexMeasOtdrParameters = Model.OtdrParametersTemplatesViewModel.Model
                .GetVeexMeasOtdrParametersBase(false)
                .FillInWithTemplate(lineParamsDto.ConnectionQuality, Model.Rtu.Omid);

            if (veexMeasOtdrParameters == null)
            {
                MeasurementCompleted?
                    .Invoke(this, new MeasurementEventArgs(ReturnCode.InvalidValueOfLmax, _trace));

                Model.IsEnabled = true;
                return;
            }

            var dto = item.TraceLeaf.Parent
                .CreateDoClientMeasurementDto(item.TraceLeaf.PortNumber, false, _readModel, Model.CurrentUser)
                .SetParams(true, Model.AutoAnalysisParamsViewModel.SearchNewEvents, false, null, veexMeasOtdrParameters);

            var startResult =
                await _grpcC2RService.SendAnyC2RRequest<DoClientMeasurementDto, ClientMeasurementStartedDto>(dto);
            if (startResult.ReturnCode != ReturnCode.MeasurementClientStartedSuccessfully)
            {
                DestroyTimer();

                MeasurementCompleted?
                    .Invoke(this, new MeasurementEventArgs(startResult.ReturnCode, _trace, startResult.ErrorMessage));

                return;
            }

            if (!Model.InterruptedPressed)
                _dispatcherProvider.GetDispatcher().Invoke(() =>
                {
                    Model.MeasurementProgressViewModel.Message =
                        Resources.SID_Measurement__Client__in_progress__Please_wait___;
                });

            _cts = new CancellationTokenSource();
            await Task.Delay(veexMeasOtdrParameters.averagingTime == @"00:05" ? 10000 : 20000);
            var veexResult = 
                await _veexMeasurementTool.Fetch(dto.RtuId, _trace, startResult.ClientMeasurementId, _cts);
            if (veexResult.Code == ReturnCode.MeasurementEndedNormally)
            {
                var res = new ClientMeasurementResultDto(veexResult.Code) { SorBytes = veexResult.SorBytes };
                ProcessMeasurementResult(res);
            }
            else
            {
                DestroyTimer();
                MeasurementCompleted?.Invoke(this, veexResult);
            }
        }

        private System.Timers.Timer _timer;
        private void StartTimer()
        {
            _logger.LogInfo(Logs.Client,$@"Start a measurement timeout for trace {_trace.Title}");
            _timer = new System.Timers.Timer(Model.MeasurementTimeout);
            _timer.Elapsed += TimeIsOver;
            _timer.AutoReset = false;
            _timer.Start();
        }

        private void DestroyTimer()
        {
            _timer.Stop();
            _timer.Dispose();
            _logger.LogInfo(Logs.Client,@"Timer destroyed");
        }

        private void TimeIsOver(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logger.LogInfo(Logs.Client,@"Measurement timeout expired");
            _timer.Dispose();

            MeasurementCompleted?
                .Invoke(this, new MeasurementEventArgs(ReturnCode.MeasurementTimeoutExpired, _trace));
        }

        public void ProcessMeasurementResult(ClientMeasurementResultDto dto)
        {
            DestroyTimer();

            _logger.LogInfo(Logs.Client,$@"Measurement (Client) result for trace {_trace.Title} received");

            if (dto.SorBytes == null)
            {
                MeasurementCompleted?.Invoke(this, new MeasurementEventArgs(dto.ReturnCode, _trace));
                return;
            }

            MeasurementCompleted?
                .Invoke(this, new MeasurementEventArgs(
                    ReturnCode.MeasurementEndedNormally, _trace, dto.SorBytes));
        }

        public async Task SetAsBaseRef(byte[] sorBytes, Trace trace)
        {
            Model.MeasurementProgressViewModel.Message = Resources.SID_Applying_base_refs__Please_wait;

            var sorData = SorData.FromBytes(sorBytes);
            var rftsParams = Model.AutoAnalysisParamsViewModel
                .GetRftsParams(sorData, Model.OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id, Model.Rtu);
            sorData.ApplyRftsParamsTemplate(rftsParams);

            _landmarksIntoBaseSetter.ApplyTraceToAutoBaseRef(sorData, trace);
            _measurementAsBaseAssigner.Initialize(Model.Rtu);
            var result = await _measurementAsBaseAssigner.Assign(sorData, trace);

            BaseRefAssigned?
                .Invoke(this, result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully
                    ? new MeasurementEventArgs(ReturnCode.BaseRefAssignedSuccessfully, trace, sorData.ToBytes())
                    : new MeasurementEventArgs(ReturnCode.BaseRefAssignmentFailed, trace, result.ErrorMessage));
        }

        public void InterruptMeasurement()
        {
            _cts?.Cancel();
        }

        public delegate void MeasurementHandler(object sender, MeasurementEventArgs e);
        public delegate void BaseRefHandler(object sender, MeasurementEventArgs e);

        public event WholeIitRtuMeasurementsExecutor.MeasurementHandler MeasurementCompleted;
        public event WholeIitRtuMeasurementsExecutor.BaseRefHandler BaseRefAssigned;
    }
}