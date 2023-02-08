using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using Microsoft.Extensions.Logging;
using Trace = Fibertest.Graph.Trace;

namespace Fibertest.WpfClient
{
    public class RtuAutoBaseViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly ILogger _logger; 
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
        private readonly FailedAutoBasePdfProvider _failedAutoBasePdfProvider;
        private readonly MonitoringSettingsModelFactory _monitoringSettingsModelFactory;
        private List<RtuAutoBaseProgress> _progress;
        public bool IsOpen { get; set; }
        public IWholeRtuMeasurementsExecutor WholeRtuMeasurementsExecutor { get; set; }
        public bool ShouldStartMonitoring { get; set; }
        private RtuLeaf _rtuLeaf;
        private Rtu _rtu;

        private List<MeasurementEventArgs> _badResults;
        private List<Trace> _goodTraces;

        private string _buttonName = Resources.SID_Close;
        public string ButtonName
        {
            get => _buttonName;
            set
            {
                if (value == _buttonName) return;
                _buttonName = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _interruptPressed;
        private bool _isInterruptEnabled = true;
        public bool IsInterruptEnabled
        {
            get => _isInterruptEnabled;
            set
            {
                if (value == _isInterruptEnabled) return;
                _isInterruptEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public RtuAutoBaseViewModel(ILifetimeScope globalScope, ILogger logger, 
            IDispatcherProvider dispatcherProvider, Model readModel, IWindowManager windowManager,
            GrpcC2DService grpcC2DService,
            IWcfServiceCommonC2D commonC2DWcfManager,
            FailedAutoBasePdfProvider failedAutoBasePdfProvider,
            MonitoringSettingsModelFactory monitoringSettingsModelFactory)
        {
            _globalScope = globalScope;
            _logger = logger;
            _dispatcherProvider = dispatcherProvider;
            _readModel = readModel;
            _windowManager = windowManager;
            _grpcC2DService = grpcC2DService;
            _commonC2DWcfManager = commonC2DWcfManager;
            _failedAutoBasePdfProvider = failedAutoBasePdfProvider;
            _monitoringSettingsModelFactory = monitoringSettingsModelFactory;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
            IsOpen = true;
            WholeRtuMeasurementsExecutor.MeasurementCompleted += MeasurementExecutor_MeasurementCompleted;
            WholeRtuMeasurementsExecutor.BaseRefAssigned += MeasurementExecutor_BaseRefAssigned;
        }

        public bool Initialize(RtuLeaf rtuLeaf)
        {
            _interruptPressed = false;
            IsInterruptEnabled = true;
            ButtonName = Resources.SID_Close;
            _finishInProgress = false;

            WholeRtuMeasurementsExecutor = rtuLeaf.RtuMaker == RtuMaker.IIT
                ? _globalScope.Resolve<WholeIitRtuMeasurementsExecutor>()
                : _globalScope.Resolve<WholeVeexRtuMeasurementsExecutor>();

            _goodTraces = new List<Trace>();
            _badResults = new List<MeasurementEventArgs>();
            var i = 0;
            _progress = rtuLeaf
                .GetAttachedTraces()
                .Select(t => new RtuAutoBaseProgress(
                    t, _readModel.Traces.First(tt => tt.TraceId == t.Id), ++i))
                .ToList();
            if (_progress.Count == 0)
                return false;

            _rtuLeaf = rtuLeaf;
            _rtu = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id);

            if (!WholeRtuMeasurementsExecutor.Initialize(_rtu))
                return false;

            // ShouldStartMonitoring = true;
            WholeRtuMeasurementsExecutor.Model.IsEnabled = true;

            return true;
        }

        private WaitCursor _waitCursor;
        public void Start()
        {
            _waitCursor = new WaitCursor();
            ButtonName = Resources.SID_Interrupt;
            WholeRtuMeasurementsExecutor.Model.IsEnabled = false;
            WholeRtuMeasurementsExecutor.Model.TraceResultsVisibility = Visibility.Visible;
            WholeRtuMeasurementsExecutor.Model.TotalTraces =
                string.Format(Resources.SID_There_are__0__trace_s__attached_to_ports_of_the_RTU, _progress.Count);

            var progressItem = _progress.First();
            Task.Factory.StartNew(() =>
                WholeRtuMeasurementsExecutor
                    .StartOneMeasurement(progressItem, progressItem.Ordinal < _progress.Count));
        }

        private string _brokenBop = "";

        private void ProcessBrokenBop(RtuAutoBaseProgress progressItem)
        {
            var otauLeaf = progressItem.TraceLeaf.Parent as OtauLeaf;
            if (otauLeaf == null) return; // impossible
            var bopAddress = otauLeaf.OtauNetAddress.Ip4Address;
            if (_brokenBop != bopAddress)
            {
                _brokenBop = bopAddress;
                Thread.Sleep(48000);
                return;
            }

            // second time the same problem 
            var thisBopTraces = _progress
                .Where(i => i.TraceLeaf.Parent is OtauLeaf o && o.OtauNetAddress.Ip4Address == _brokenBop).ToList();
            foreach (var item in thisBopTraces)
            {
                item.MeasurementDone = true;
                item.BaseRefAssigned = true;
                if (item != progressItem)
                    _badResults.Add(new MeasurementEventArgs(ReturnCode.RtuToggleToBopPortError, item.Trace));
            }

            _brokenBop = "";
        }

        private async void MeasurementExecutor_MeasurementCompleted(object sender, MeasurementEventArgs result)
        {
            _logger.LogInfo(Logs.Client,$@"Measurement on trace {result.Trace!.Title}: {result.Code}");
            var progressItem = _progress.First(i => i.Trace.TraceId == result.Trace.TraceId);
            if (result.Code == ReturnCode.RtuToggleToBopPortError)
            {
                ProcessBrokenBop(progressItem);
            }
            else if (result.Code == ReturnCode.MeasurementInterrupted)
            {
                await TryCloseAsync();
                return;
            }
            else
                progressItem.MeasurementDone =
                    result.Code != ReturnCode.D2RGrpcOperationError &&
                    result.Code != ReturnCode.RtuInitializationInProgress &&
                    result.Code != ReturnCode.RtuAutoBaseMeasurementInProgress;

            if (progressItem.MeasurementDone)
            {
                var line = $@"{progressItem.Ordinal}/{_progress.Count} {result.Trace.Title} : {result.Code.RtuAutoBaseStyle()}";
                _dispatcherProvider.GetDispatcher().Invoke(() =>
                {
                    WholeRtuMeasurementsExecutor.Model.TraceResults.Add(line);
                }); // sync, GUI thread

                if (result.Code != ReturnCode.MeasurementEndedNormally)
                {
                    _badResults.Add(result);
                    progressItem.BaseRefAssigned = true; // nothing to assign
                }
                else
                {
                    _logger.LogInfo(Logs.Client,$@"Assign base refs for {result.Trace.Title}");
                    await Task.Factory.StartNew(
                        () => WholeRtuMeasurementsExecutor.SetAsBaseRef(result.SorBytes!, result.Trace));
                }
            }
            else
            {
                Thread.Sleep(7000);
            }

            await StartNextMeasurement();
        }

        private async Task StartNextMeasurement()
        {
            var nextItem = _progress.FirstOrDefault(i => !i.MeasurementDone);
            if (!_interruptPressed && nextItem != null)
            {
                Thread.Sleep(100);
                await Task.Factory.StartNew(() =>
                    WholeRtuMeasurementsExecutor.StartOneMeasurement(nextItem, nextItem.Ordinal < _progress.Count));
            }
            else
            {
                if (_interruptPressed || _progress.All(i => i.MeasurementDone && i.BaseRefAssigned))
                {
                    await _dispatcherProvider.GetDispatcher().Invoke(Finish);
                }
            }
        }

        private void MeasurementExecutor_BaseRefAssigned(object sender, MeasurementEventArgs result)
        {
            ProcessBaseRefAssignedResult(result);
        }

        private async void ProcessBaseRefAssignedResult(MeasurementEventArgs result)
        {
            var progressItem = _progress.First(i => i.Trace.TraceId == result.Trace!.TraceId);
            progressItem.BaseRefAssigned = true;
            _logger.LogInfo(Logs.Client,$@"Assigned base ref for trace {result.Trace!.Title}: {result.Code}");

            var line = $@"{progressItem.Ordinal}/{_progress.Count} {result.Trace.Title} : {result.Code.RtuAutoBaseStyle()}";
            _dispatcherProvider.GetDispatcher().Invoke(() =>
            {
                WholeRtuMeasurementsExecutor.Model.TraceResults.Add(line);
            });

            if (result.Code != ReturnCode.BaseRefAssignedSuccessfully)
                _badResults.Add(result);
            else
                _goodTraces.Add(result.Trace);

            if (_interruptPressed || _progress.All(i => i.MeasurementDone && i.BaseRefAssigned))
            {
                await _dispatcherProvider.GetDispatcher().Invoke(Finish);
            }
        }

        private bool _finishInProgress;
        private async Task Finish()
        {
            if (_finishInProgress) return;
            _finishInProgress = true;
            _logger.LogInfo(Logs.Client,@"Terminating process...");

            _waitCursor.Dispose();
            if (_rtu.RtuMaker == RtuMaker.IIT)
            {
                var r = await _commonC2DWcfManager.FreeOtdrAsync(
                    new FreeOtdrDto(_rtu.Id, _rtu.RtuMaker));
                _logger.LogInfo(Logs.Client,$@"Free OTDR result is {r.ReturnCode}");
            }

            if (_interruptPressed)
            {
                await TryCloseAsync();
                return;
            }

            if (!_interruptPressed && _badResults.Any())
                ShowReport();

            string startMonitoringResult = "";
            if (!_interruptPressed && ShouldStartMonitoring && _goodTraces.Any())
            {
                startMonitoringResult = await StartMonitoring();
            }

            WholeRtuMeasurementsExecutor.Model.MeasurementProgressViewModel.DisplayStop();

            if (!_interruptPressed)
            {
                var timestamp = $@"{DateTime.Now:d} {DateTime.Now:t}";
                var strs = new List<string>() { Resources.SID_The_process_of_setting_base_ref_for_RTU, _rtu.Title, Resources.SID_is_completed_at_ + timestamp };
                if (ShouldStartMonitoring && _goodTraces.Any())
                {
                    strs.Add("");
                    strs.Add(string.IsNullOrEmpty(startMonitoringResult) ? Resources.SID_Monitoring_settings_applied_successfully_ : startMonitoringResult);
                }

                var mb = new MyMessageBoxViewModel(MessageType.Information, strs);
                await _windowManager.ShowDialogWithAssignedOwner(mb);
            }

            WholeRtuMeasurementsExecutor.Model.IsEnabled = true;
            WholeRtuMeasurementsExecutor.Model.TraceResults.Clear();
            WholeRtuMeasurementsExecutor.Model.TraceResultsVisibility = Visibility.Collapsed;
            await TryCloseAsync();
        }

        private async Task<string> StartMonitoring()
        {
            WholeRtuMeasurementsExecutor.Model.MeasurementProgressViewModel.Message1 = 
                Resources.SID_Starting_monitoring;
            WholeRtuMeasurementsExecutor.Model.MeasurementProgressViewModel.Message = 
                Resources.SID_Sending_command__Wait_please___;
            var monitoringSettingsModel = _monitoringSettingsModelFactory.Create(_rtuLeaf, false);

            using (_globalScope.Resolve<IWaitCursor>())
            {
                var dto = monitoringSettingsModel.CreateDto();
                dto.Ports = _goodTraces
                    .Select(trace => new PortWithTraceDto(trace.OtauPort, trace.TraceId)).ToList();
                dto.IsMonitoringOn = true;

                var resultDto = await _commonC2DWcfManager.ApplyMonitoringSettingsAsync(dto);
                if (resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
                {
                    var cmd = dto.CreateCommand();
                    var result = await _grpcC2DService.SendEventSourcingCommand(cmd);
                    return result.ReturnCode == ReturnCode.Ok ? "" : result.ErrorMessage!;
                }
                else
                {
                    return Resources.SID_Failed_to_apply_monitoring_settings_;
                }
            }
        }

        private void ShowReport()
        {
            var htmlContent = _failedAutoBasePdfProvider.Create(_rtu, _badResults);
            if (htmlContent == null) return;

            var pdfFileName = htmlContent.SaveHtmlAsPdf("FailedAutoBaseMeasurementsReport");
            Process.Start(new ProcessStartInfo() { FileName = pdfFileName, UseShellExecute = true });
        }

        // button Close or Interrupt
        public async void Close()
        {
            if (ButtonName == Resources.SID_Close)
                await TryCloseAsync();
            else
            {
                _interruptPressed = true;
                WholeRtuMeasurementsExecutor.Model.InterruptedPressed = true;
                WholeRtuMeasurementsExecutor.InterruptMeasurement(); 
                IsInterruptEnabled = false;
                ButtonName = Resources.SID_Wait___;
                WholeRtuMeasurementsExecutor.Model.MeasurementProgressViewModel.DisplayFinishInProgress();
                _logger.LogInfo(Logs.Client,@"Interrupt process pressed...");
            }
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            IsOpen = false;
            WholeRtuMeasurementsExecutor.MeasurementCompleted -= MeasurementExecutor_MeasurementCompleted;
            WholeRtuMeasurementsExecutor.BaseRefAssigned -= MeasurementExecutor_BaseRefAssigned;

            await _globalScope.Resolve<IRtuHolder>().SetRtuOccupationState(_rtu.Id, _rtu.Title, RtuOccupation.None);
            return true;
        }
    }
}
