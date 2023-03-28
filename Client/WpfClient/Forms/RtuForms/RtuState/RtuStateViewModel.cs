using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class RtuStateViewModel : Screen
    {
        private readonly SoundManager _soundManager;
        private readonly RtuStateModelFactory _rtuStateModelFactory;
        private bool _isSoundForThisVmInstanceOn;
        private bool _isUserAskedToOpenView;
        private RtuPartStateChanges _changes;

        public bool IsOpen { get; private set; }

        private RtuStateModel _model = null!;
        public RtuStateModel Model
        {
            get { return _model; }
            set
            {
                if (Equals(value, _model)) return;
                _model = value;
                NotifyOfPropertyChange();
            }
        }

        public RtuStateViewModel(SoundManager soundManager, RtuStateModelFactory rtuStateModelFactory)
        {
            _soundManager = soundManager;
            _rtuStateModelFactory = rtuStateModelFactory;
        }

        public void Initialize(RtuStateModel model, bool isUserAskedToOpenView, RtuPartStateChanges changes)
        {
            Model = model;
            _isUserAskedToOpenView = isUserAskedToOpenView;
            _changes = changes;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_State_of_RTU;
            IsOpen = true;

            if (_isUserAskedToOpenView)
                return;

            if (_changes == RtuPartStateChanges.DifferentPartsHaveDifferentChanges || _changes == RtuPartStateChanges.OnlyBetter)
                _soundManager.PlayOk();

            if (_changes == RtuPartStateChanges.DifferentPartsHaveDifferentChanges || _changes == RtuPartStateChanges.OnlyWorse)
            {
                _isSoundForThisVmInstanceOn = true;
                _soundManager.StartAlert();
                Model.IsSoundButtonEnabled = true;
            }
        }

        public void RefreshModel(RtuLeaf rtuLeaf)
        {
            Model = _rtuStateModelFactory.Create(rtuLeaf)!;
        }

        public void MonitoringStarted()
        {
            Model.MonitoringMode = MonitoringState.On.ToLocalizedString();
        }

        public void MonitoringStopped()
        {
            Model.MonitoringMode = MonitoringState.Off.ToLocalizedString();
            Model.CurrentMeasurementStep = BuildMessage(MonitoringCurrentStep.Idle);
        }

        public void NotifyUserMonitoringResult(MeasurementAdded dto)
        {
            var portLineVm = Model.Ports.FirstOrDefault(p => p.TraceId == dto.TraceId);
            if (portLineVm == null)
                return;

            portLineVm.TraceState = dto.BaseRefType == BaseRefType.Fast && dto.TraceState != FiberState.Ok? FiberState.Suspicion : dto.TraceState;
            portLineVm.Timestamp = dto.MeasurementTimestamp;
            portLineVm.LastSorFileId = dto.SorFileId.ToString();

            Model.SetWorstTraceStateAsAggregate();

            if (Model.MonitoringMode == MonitoringState.Off.ToLocalizedString())
                Model.CurrentMeasurementStep = Resources.SID_No_measurement;
        }

        public void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            string portName = "";
            string traceTitle = "";

            if (dto.PortWithTraceDto != null)
            {
                var portLineVm = Model.Ports.FirstOrDefault(p => p.TraceId == dto.PortWithTraceDto.TraceId);
                if (portLineVm != null)
                {
                    traceTitle = portLineVm.TraceTitle!;
                    portName = portLineVm.Number;
                }
            }

            Model.CurrentMeasurementStep = BuildMessage(dto.Step, portName, traceTitle);
        }

        private string BuildMessage(MonitoringCurrentStep step, string portName = "", string traceTitle = "")
        {
            switch (step)
            {
                case MonitoringCurrentStep.Idle:
                    return Resources.SID_No_measurement;
                case MonitoringCurrentStep.Toggle:
                   return string.Format(Resources.SID_Toggling_to_the_port__0_, portName);
                case MonitoringCurrentStep.Measure:
                    return string.Format(Resources.SID_Measurement_on_port__0___trace___1__, portName, traceTitle);
                case MonitoringCurrentStep.FailedOtauProblem:
                    return string.Format(Resources.SID_Measurement_on_port__0___trace___1___failed__OTAU_problem, portName,
                        traceTitle);
                case MonitoringCurrentStep.FailedOtdrProblem:
                    return string.Format(Resources.SID_Measurement_on_port__0___trace___1___failed__OTDR_problem, portName,
                        traceTitle);
                case MonitoringCurrentStep.Analysis:
                    return string.Format(Resources.SID_Measurement_s_result_analysis__port__0____trace___1__, portName, traceTitle);
                case MonitoringCurrentStep.Interrupted:
                    return Resources.SID_Measurement_interrupted;
                case MonitoringCurrentStep.MeasurementFinished:
                    return string.Format(Resources.SID_Measurement_on_port__0__trace__1__is_finished, portName, traceTitle);
                default:
                    return Resources.SID_Unknown;
            }
        }

        public void TurnSoundOff()
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            _isSoundForThisVmInstanceOn = false;
            Model.IsSoundButtonEnabled = false;
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await Task.Delay(0);
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            IsOpen = false;
            return true;
        }
        public async void Close()
        {
            await TryCloseAsync();
        }
    }
}
