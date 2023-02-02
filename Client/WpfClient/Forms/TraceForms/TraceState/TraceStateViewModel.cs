using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Graph.TraceStateReport;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using Fibertest.StringResources;
using GMap.NET;
using Microsoft.Extensions.Logging;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class TraceStateViewModel : Screen
    {
        private readonly ILogger _logger; 
        private readonly CurrentUser _currentUser;
        private readonly CurrentGis _currentGis;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly SoundManager _soundManager;
        private readonly Model _readModel;
        private readonly CommandLineParameters _commandLineParameters;
        private readonly IWcfServiceInSuperClient _c2SWcfManager;
        private readonly TabulatorViewModel _tabulatorViewModel;
        private readonly TraceStateReportProvider _traceStateReportProvider;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly LandmarksViewsManager _landmarksViewsManager;
        private readonly GraphReadModel _graphReadModel;
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private bool _isSoundForThisVmInstanceOn;
        private bool _isTraceStateChanged;
        public bool IsOpen { get; private set; }

        public TraceStateModel Model { get; set; }

        public List<EventStatusComboItem> StatusRows { get; set; }
        public EventStatusComboItem SelectedEventStatus { get; set; }
        public bool HasPrivilegies { get; set; }

        private bool _isEditEnabled;
        public bool IsEditEnabled
        {
            get => _isEditEnabled;
            set
            {
                if (value == _isEditEnabled) return;
                _isEditEnabled = value;
                NotifyOfPropertyChange();
            }
        }


        public TraceStateViewModel(ILogger logger, CurrentUser currentUser, CurrentGis currentGis,
             ReflectogramManager reflectogramManager,
            SoundManager soundManager, Model readModel, GraphReadModel graphReadModel,
            GrpcC2DRequests grpcC2DRequests, IWcfServiceInSuperClient c2SWcfManager, 
            CommandLineParameters commandLineParameters, 
            TabulatorViewModel tabulatorViewModel, TraceStateReportProvider traceStateReportProvider,
            TraceStatisticsViewsManager traceStatisticsViewsManager, LandmarksViewsManager landmarksViewsManager)
        {
            _logger = logger;
            _currentUser = currentUser;
            _currentGis = currentGis;
            HasPrivilegies = currentUser.Role <= Role.Operator;
            IsEditEnabled = true;
            _reflectogramManager = reflectogramManager;
            _soundManager = soundManager;
            _readModel = readModel;
            _commandLineParameters = commandLineParameters;
            _c2SWcfManager = c2SWcfManager;
            _tabulatorViewModel = tabulatorViewModel;
            _traceStateReportProvider = traceStateReportProvider;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _landmarksViewsManager = landmarksViewsManager;
            _graphReadModel = graphReadModel;
            _grpcC2DRequests = grpcC2DRequests;
        }

        public void Initialize(TraceStateModel model, bool isTraceStateChanged)
        {
            Model = model;
            _isTraceStateChanged = isTraceStateChanged;
            // if (Model.Accidents.Count > 0)
            Model.SelectedAccident = Model.Accidents.FirstOrDefault();
            if (Model.EventStatus > EventStatus.EventButNotAnAccident)
                InitializeEventStatusCombobox();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Trace_state;
            IsOpen = true;

            if (!_isTraceStateChanged)
                return;

            if (Model.TraceState != FiberState.Ok)
            {
                _isSoundForThisVmInstanceOn = true;
                _soundManager.StartAlert();
                Model.IsSoundButtonEnabled = true;
            }
            else
            {
                _soundManager.PlayOk();
            }
        }

        private void InitializeEventStatusCombobox()
        {
            StatusRows = new List<EventStatusComboItem>
            {  // not foreach because order matters
                new EventStatusComboItem() {EventStatus = EventStatus.Confirmed},
                new EventStatusComboItem() {EventStatus = EventStatus.NotConfirmed},
                new EventStatusComboItem() {EventStatus = EventStatus.Planned},
                new EventStatusComboItem() {EventStatus = EventStatus.Suspended},
                new EventStatusComboItem() {EventStatus = EventStatus.NotImportant},
                new EventStatusComboItem() {EventStatus = EventStatus.Unprocessed},
            };

            SelectedEventStatus = StatusRows.FirstOrDefault(r => r.EventStatus == Model.EventStatus);
        }


        //----
        public void TurnSoundOff()
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            _isSoundForThisVmInstanceOn = false;
            Model.IsSoundButtonEnabled = false;
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            IsOpen = false;
            return true;
        }

        public async void ShowAccidentPlace()
        {
            PointLatLng? accidentPoint;
            if (Model.Accidents.Count == 0 || Model.SelectedAccident == null)
                accidentPoint = Model.Header.RtuPosition;
            else accidentPoint = Model.SelectedAccident.Position;

            if (_tabulatorViewModel.SelectedTabIndex != 3)
                _tabulatorViewModel.SelectedTabIndex = 3;

            await Task.Delay(100);

            if (_currentGis.ThresholdZoom > _graphReadModel.MainMap.Zoom)
                _graphReadModel.MainMap.Zoom = _currentGis.ThresholdZoom;
            _graphReadModel.ExtinguishAllNodes();

            if (accidentPoint != null)
                _graphReadModel.PlacePointIntoScreenCenter((PointLatLng)accidentPoint);

            if (_commandLineParameters.IsUnderSuperClientStart)
            {
                _logger.LogInfo(Logs.Client,$@"Ask super-client to switch onto this system (postfix = {_commandLineParameters.ClientOrdinal}).");
                await _c2SWcfManager.SwitchOntoSystem(_commandLineParameters.ClientOrdinal);
            }
        }

        public void ShowReflectogram()
        {
            _reflectogramManager.SetTempFileName(Model.Header.TraceTitle, Model.SorFileId, Model.MeasurementTimestamp);
            _reflectogramManager.ShowRefWithBase(Model.SorFileId);
        }
        public void ShowRftsEvents() { _reflectogramManager.ShowRftsEvents(Model.SorFileId, Model.Header.TraceTitle); }
        public void ShowTraceStatistics() { _traceStatisticsViewsManager.Show(Model.TraceId); }
        public void ExportToKml() { }

        public void ShowHtmlReport()
        {
            var reportModel = CreateReportModelFromMeasurement();
            var htmlFile = EventReport.FillInHtmlReportForTraceState(reportModel);
            try
            {
                Process.Start(htmlFile);
            }
            catch (Exception e)
            {
                _logger.LogError(Logs.Client,@"ShowReport: " + e.Message);
            }
        }

        public async void ShowLandmarks()
        {
            var rtuNodeId = _readModel.Traces.First(t => t.TraceId == Model.TraceId).NodeIds[0];
            await _landmarksViewsManager.InitializeFromTrace(Model.TraceId, rtuNodeId);
        }

        public void ShowReport()
        {
            var reportModel = new TraceReportModel()
            {
                TraceTitle = Model.Header.TraceTitle,
                TraceState = Model.TraceState.ToLocalizedString(),
                RtuTitle = Model.Header.RtuTitle,
                RtuSoftwareVersion = Model.Header.RtuSoftwareVersion,
                PortTitle = Model.Header.PortTitle,
                MeasurementTimestamp = $@"{Model.MeasurementTimestamp:G}",
                RegistrationTimestamp = $@"{Model.RegistrationTimestamp:G}",

                Accidents = Model.Accidents,
            };
            // var report = _traceStateReportProvider.Create(reportModel, _currentDatacenterParameters);
            //
            // PdfExposer.Show(report, @"TraceStateReport.pdf", _windowManager);
        }

        public async void SaveMeasurementChanges()
        {
            using (new WaitCursor())
            {
                IsEditEnabled = false;
                var dto = new UpdateMeasurement
                {
                    SorFileId = Model.SorFileId,
                    Comment = Model.Comment,
                };

                if (Model.OpticalEventPanelVisibility == Visibility.Visible && Model.EventStatus != SelectedEventStatus.EventStatus)
                {
                    dto.EventStatus = SelectedEventStatus.EventStatus;
                    dto.StatusChangedTimestamp = DateTime.Now;
                    dto.StatusChangedByUser = _currentUser.UserName;
                }
                else
                {
                    dto.EventStatus = Model.EventStatus;
                    var measurement = _readModel.Measurements.FirstOrDefault(m => m.SorFileId == Model.SorFileId);
                    if (measurement != null)
                    {
                        dto.StatusChangedTimestamp = measurement.StatusChangedTimestamp;
                        dto.StatusChangedByUser = measurement.StatusChangedByUser;
                    }
                }

                var result = await _grpcC2DRequests.SendEventSourcingCommand(dto);
                if (result.ReturnCode != ReturnCode.Ok)
                    _logger.LogInfo(Logs.Client,@"Cannot update measurement!");
                IsEditEnabled = true;
            }
            await TryCloseAsync();
        }

        private EventReportModel CreateReportModelFromMeasurement()
        {
            return new EventReportModel()
            {
                TraceTitle = Model.Trace.Title,
                TraceState = Model.TraceState.ToLocalizedString(),
                RtuTitle = Model.Header.RtuTitle,
                Port = Model.Header.PortTitle,
                TimeStamp = Model.MeasurementTimestamp,
            };
        }

        public async void Close() { await TryCloseAsync(); }
    }
}

