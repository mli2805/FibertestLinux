using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.OtdrDataFormat;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using GrpsClientLib;
using Landmark = Fibertest.Graph.Landmark;

namespace Fibertest.WpfClient
{
    public class LandmarksViewModel : Screen
    {
        private string _rtuTitle = null!;
        public CurrentGis CurrentGis { get; }
        private bool _isLandmarksFromBase;
        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
            (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
             select new GpsInputModeComboItem(mode)).ToList();

        public List<Trace> Traces { get; set; } = null!;

        private Trace _selectedTrace = null!;
        public Trace SelectedTrace
        {
            get => _selectedTrace;
            set
            {
                if (Equals(value, _selectedTrace)) return;
                _selectedTrace = value;
            }
        }

        private GpsInputModeComboItem _selectedGpsInputMode;
        public GpsInputModeComboItem SelectedGpsInputMode
        {
            get => _selectedGpsInputMode;
            set
            {
                if (Equals(value, _selectedGpsInputMode)) return;
                _selectedGpsInputMode = value;
                CurrentGis.GpsInputMode = _selectedGpsInputMode.Mode;
                RefreshCoorsInRows();
            }
        }

        private void RefreshCoorsInRows()
        {
            foreach (var row in Rows)
            {
                var landmark = _landmarks.First(l => l.NodeId == row.NodeId);
                row.GpsCoors = landmark.GpsCoors.ToDetailedString(_selectedGpsInputMode.Mode);
            }
        }

        private ObservableCollection<LandmarkRow> LandmarksToRows()
        {
            var temp = _isFilterOn ? _landmarks.Where(l => l.EquipmentType != EquipmentType.EmptyNode) : _landmarks;
            return new ObservableCollection<LandmarkRow>(temp.Select(l =>  new LandmarkRow().FromLandmark(l, _selectedGpsInputMode.Mode)));
        }

        private bool _isFilterOn;
        public bool IsFilterOn
        {
            get => _isFilterOn;
            set
            {
                if (value == _isFilterOn) return;
                _isFilterOn = value;
                Rows = LandmarksToRows();
            }
        }

        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly LandmarksBaseParser _landmarksBaseParser;
        private readonly LandmarksGraphParser _landmarksGraphParser;
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly IWindowManager _windowManager;
        private List<Landmark> _landmarks = null!;

        private ObservableCollection<LandmarkRow> _rows = null!;
        public ObservableCollection<LandmarkRow> Rows
        {
            get => _rows;
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        private LandmarkRow _selectedRow = null!;
        public LandmarkRow SelectedRow
        {
            get => _selectedRow;
            set
            {
                _selectedRow = value;
                InitiateOneLandmarkControl();
                NotifyOfPropertyChange();
            }
        }

        private async void InitiateOneLandmarkControl()
        {
            await OneLandmarkViewModel.Cancel(OneLandmarkViewModel.BeforeNew);
            var landmark = _landmarks.First(l => l.Number == SelectedRow.Number);
            OneLandmarkViewModel.SelectedLandmark = landmark.Clone();

            if (_isLandmarksFromBase || SelectedRow.Number == 0 || SelectedRow.Number == _landmarks.Last().Number)
            {
                OneLandmarkViewModel.IsIncludeEquipmentEnabled = false;
                OneLandmarkViewModel.IsExcludeEquipmentEnabled = false;
            }
            else
            {
                OneLandmarkViewModel.IsIncludeEquipmentEnabled = landmark.EquipmentType == EquipmentType.EmptyNode;
                OneLandmarkViewModel.IsExcludeEquipmentEnabled = !OneLandmarkViewModel.IsIncludeEquipmentEnabled;
            }

            OneLandmarkViewModel.IsFromBaseRef = _isLandmarksFromBase;
        }

        private OneLandmarkViewModel _oneLandmarkViewModel = null!;
        public OneLandmarkViewModel OneLandmarkViewModel
        {
            get => _oneLandmarkViewModel;
            set
            {
                if (Equals(value, _oneLandmarkViewModel)) return;
                _oneLandmarkViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility GisVisibility { get; set; }
        public int DataGridWidth { get; set; }

        public LandmarksViewModel(ILifetimeScope globalScope, Model readModel, CurrentGis currentGis,
            LandmarksBaseParser landmarksBaseParser, LandmarksGraphParser landmarksGraphParser,
             GrpcC2DRequests grpcC2DRequests, IWcfServiceCommonC2D c2DWcfCommonManager, IWindowManager windowManager)
        {
            CurrentGis = currentGis;
            _globalScope = globalScope;
            _readModel = readModel;
            _landmarksBaseParser = landmarksBaseParser;
            _landmarksGraphParser = landmarksGraphParser;
            _grpcC2DRequests = grpcC2DRequests;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _windowManager = windowManager;
            _selectedGpsInputMode = GpsInputModes.First(i => i.Mode == CurrentGis.GpsInputMode);
            GisVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            DataGridWidth = currentGis.IsGisOn ? 770 : 570;
        }

        private async Task<int> Initialize()
        {
            OneLandmarkViewModel = _globalScope.Resolve<OneLandmarkViewModel>();
            OneLandmarkViewModel.RtuId = _selectedTrace.RtuId;
            var res = await PrepareLandmarks();
            SelectedRow = Rows.First();
            return res;
        }

        public async Task<int> InitializeFromRtu(Guid rtuId)
        {
            _rtuTitle = _readModel.Rtus.First(r => r.Id == rtuId).Title;
            Traces = _readModel.Traces.Where(t => t.RtuId == rtuId).ToList();
            if (Traces.Count == 0) return -1;
            _selectedTrace = Traces.First();

            return await Initialize();
        }

        public async Task<int> InitializeFromTrace(Guid traceId, Guid selectedNodeId)
        {
            var trace = _readModel.Traces.First(t => t.TraceId == traceId);
            _rtuTitle = _readModel.Rtus.First(r => r.Id == trace.RtuId).Title;
            Traces = _readModel.Traces.Where(t => t.RtuId == trace.RtuId).ToList();
            _selectedTrace = _readModel.Traces.First(t => t.TraceId == traceId);

            var res = await Initialize();
            SelectedRow = Rows.First(r => r.NodeId == selectedNodeId);
            return res;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Landmarks_of_traces_of_RTU_ + _rtuTitle;
        }

        private async Task<int> PrepareLandmarks()
        {
            OneLandmarkViewModel.TraceTitle = SelectedTrace.Title;
            _isLandmarksFromBase = SelectedTrace.PreciseId != Guid.Empty;
            if (_isLandmarksFromBase)
            {
                var sorData = await GetBase(SelectedTrace.PreciseId);
                _landmarks = sorData != null 
                    ? _landmarksBaseParser.GetLandmarks(sorData, SelectedTrace) 
                    : new List<Landmark>();
            }
            else
            {
                _landmarks = _landmarksGraphParser.GetLandmarks(SelectedTrace);
            }

            Rows = LandmarksToRows();
            return 0;
        }

        private async Task<OtdrDataKnownBlocks?> GetBase(Guid baseId)
        {
            if (baseId == Guid.Empty)
                return null;

            var baseRef = _readModel.BaseRefs.First(b => b.Id == baseId);
            OneLandmarkViewModel.SorFileId = baseRef.SorFileId;
            OneLandmarkViewModel.PreciseTimestamp = baseRef.SaveTimestamp;
            var sorBytes = await _c2DWcfCommonManager.GetSorBytes(baseRef.SorFileId);
            return SorData.FromBytes(sorBytes);
        }

        public async Task<int> RefreshOrChangeTrace() // button
        {
            OneLandmarkViewModel = _globalScope.Resolve<OneLandmarkViewModel>();
            OneLandmarkViewModel.RtuId = _selectedTrace.RtuId;
            await PrepareLandmarks();
            SelectedRow = Rows.First();
            return 0;
        }

        public async void RefreshAsChangesReaction()
        {
            var index = Rows.IndexOf(SelectedRow);

            await PrepareLandmarks();
            OneLandmarkViewModel = _globalScope.Resolve<OneLandmarkViewModel>();
            OneLandmarkViewModel.RtuId = _selectedTrace.RtuId;
            SelectedRow = Rows[index];
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await OneLandmarkViewModel.Cancel(OneLandmarkViewModel.CanClose);
            return true;
        }

        public async void IncludeEquipment()
        {
            var node = _readModel.Nodes.First(n => n.NodeId == SelectedRow.NodeId);
            var allEquipmentInNode = _readModel.Equipments.Where(e => e.NodeId == node.NodeId).ToList();
            var traceContentChoiceViewModel = _globalScope.Resolve<TraceContentChoiceViewModel>();
            traceContentChoiceViewModel.Initialize(allEquipmentInNode, node, false);
            await _windowManager.ShowDialogWithAssignedOwner(traceContentChoiceViewModel);
            if (!traceContentChoiceViewModel.ShouldWeContinue || 
                traceContentChoiceViewModel.GetSelectedEquipmentGuid() == SelectedRow.EquipmentId) return;

            var cmd = new IncludeEquipmentIntoTrace()
                {
                    TraceId = SelectedTrace.TraceId,
                    IndexInTrace = SelectedRow.NumberIncludingAdjustmentPoints,
                    EquipmentId = traceContentChoiceViewModel.GetSelectedEquipmentGuid()
                };
            await _grpcC2DRequests.SendEventSourcingCommand(cmd);
        }

        public async void ExcludeEquipment()
        {
            var cmd = new ExcludeEquipmentFromTrace()
            {
                TraceId = SelectedTrace.TraceId,
                IndexInTrace = SelectedRow.NumberIncludingAdjustmentPoints,
                EquipmentId = SelectedRow.EquipmentId,
            };
            await _grpcC2DRequests.SendEventSourcingCommand(cmd);
        }

        public void ExportToPdf()
        {
            // var report = LandmarksReportProvider.Create(_landmarks, _selectedTrace.Title, _selectedGpsInputMode.Mode);
            // PdfExposer.Show(report, $@"Landmarks {_selectedTrace.Title}.pdf", _windowManager);
        }
    }
}
