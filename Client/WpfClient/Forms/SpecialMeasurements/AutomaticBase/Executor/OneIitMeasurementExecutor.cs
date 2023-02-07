using System.Linq;
using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using GrpsClientLib;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class OneIitMeasurementExecutor : IOneMeasurementExecutor
    {
        private readonly ILogger _logger; 
        private readonly Model _readModel;
        private readonly GrpcC2RRequests _grpcC2RRequests;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly LandmarksIntoBaseSetter _landmarksIntoBaseSetter;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;

        private Trace _trace = null!;
        public MeasurementModel Model { get; set; } = new MeasurementModel();

        public OneIitMeasurementExecutor(IWritableConfig<ClientConfig> config, ILogger logger, CurrentUser currentUser, Model readModel,
            GrpcC2RRequests grpcC2RRequests, IDispatcherProvider dispatcherProvider,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            LandmarksIntoBaseSetter landmarksIntoBaseSetter, MeasurementAsBaseAssigner measurementAsBaseAssigner
            )
        {
            _logger = logger;
            _readModel = readModel;
            _grpcC2RRequests = grpcC2RRequests;
            _dispatcherProvider = dispatcherProvider;
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

            var dto = traceLeaf.Parent
                .CreateDoClientMeasurementDto(traceLeaf.PortNumber, keepOtdrConnection, _readModel, Model.CurrentUser)
                .SetParams(true, Model.AutoAnalysisParamsViewModel.SearchNewEvents, Model.OtdrParametersTemplatesViewModel.IsAutoLmaxSelected(),
                    Model.OtdrParametersTemplatesViewModel.GetSelectedParameters(),
                    Model.OtdrParametersTemplatesViewModel.GetVeexSelectedParameters());

            var startResult =
                await _grpcC2RRequests.SendAnyC2RRequest<DoClientMeasurementDto, ClientMeasurementStartedDto>(dto);
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

           // RtuMaker is IIT - result will come through WCF contract
        }

        private System.Timers.Timer _timer = null!;
        private void StartTimer()
        {
            _logger.LogInfo(Logs.Client,@"Start a measurement timeout");
            _timer = new System.Timers.Timer(Model.MeasurementTimeout);
            _timer.Elapsed += TimeIsOver;
            _timer.AutoReset = false;
            _timer.Start();
        }
        private void TimeIsOver(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _logger.LogInfo(Logs.Client,@"Measurement timeout expired");
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

            _logger.LogInfo(Logs.Client,@"Measurement (Client) result received");

            Model.MeasurementProgressViewModel.Message = Resources.SID_Applying_base_refs__Please_wait;

            var sorData = SorData.FromBytes(dto.SorBytes);
            var rftsParams = Model.AutoAnalysisParamsViewModel
                .GetRftsParams(sorData, Model.OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id, Model.Rtu);
            sorData.ApplyRftsParamsTemplate(rftsParams);

            _landmarksIntoBaseSetter.ApplyTraceToAutoBaseRef(sorData, _trace);
            _measurementAsBaseAssigner.Initialize(Model.Rtu);
            var result = await _measurementAsBaseAssigner.Assign(sorData, _trace);

            MeasurementCompleted?
                .Invoke(this, result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully
                    ? new MeasurementEventArgs(ReturnCode.BaseRefAssignedSuccessfully, _trace, sorData.ToBytes())
                    : new MeasurementEventArgs(ReturnCode.BaseRefAssignmentFailed, _trace, result.ErrorMessage ?? ""));
        }

        public delegate void MeasurementHandler(object sender, MeasurementEventArgs e);

        public event MeasurementHandler? MeasurementCompleted;
    }
}
