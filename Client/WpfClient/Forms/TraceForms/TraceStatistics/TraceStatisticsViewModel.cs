using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class TraceStatisticsViewModel : Screen
    {
        private readonly Model _readModel;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly BaseRefModelFactory _baseRefModelFactory;

        private Trace? _trace;
        public bool IsOpen { get; private set; }

        public string TraceTitle { get; set; } = null!;
        public string RtuTitle { get; set; } = null!;
        public string PortNumber { get; set; } = null!;

        private BaseRefModel? _selectedBaseRef;
        public BaseRefModel? SelectedBaseRef
        {
            get { return _selectedBaseRef; }
            set
            {
                if (Equals(value, _selectedBaseRef)) return;
                _selectedBaseRef = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<BaseRefModel> BaseRefs { get; set; } = new();

        public ObservableCollection<TraceMeasurementModel> Rows { get; set; } = new();

        private TraceMeasurementModel? _selectedRow;
        public TraceMeasurementModel? SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public TraceStatisticsViewModel(Model readModel, ReflectogramManager reflectogramManager,
            TraceStateViewsManager traceStateViewsManager, BaseRefModelFactory baseRefModelFactory)
        {
            _readModel = readModel;
            _reflectogramManager = reflectogramManager;
            _traceStateViewsManager = traceStateViewsManager;
            _baseRefModelFactory = baseRefModelFactory;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Measurement.SorFileId", ListSortDirection.Descending));
        }

        public void Initialize(Guid traceId)
        {
            _trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (_trace == null)
                return;
            TraceTitle = _trace.Title;
            RtuTitle = _readModel.Rtus.First(r => r.Id == _trace.RtuId).Title;
            PortNumber = _trace.OtauPort == null ? Resources.SID__not_attached_ : _trace.OtauPort.IsPortOnMainCharon
                ? _trace.OtauPort.OpticalPort.ToString()
                : $@"{_trace.OtauPort.Serial}-{_trace.OtauPort.OpticalPort}";

            BaseRefs.Clear();
            foreach (var baseRef in _readModel.BaseRefs.Where(b => b.TraceId == traceId))
            {
                BaseRefs.Add(_baseRefModelFactory.Create(baseRef));
            }

            Rows.Clear();
            foreach (var measurement in _readModel.Measurements
                         .Where(m => m.TraceId == traceId).OrderBy(t => t.MeasurementTimestamp))
                Rows.Add(new TraceMeasurementModel(measurement));
        }

        public void AddNewMeasurement()
        {
            var lastMeasurement = _readModel.Measurements.Last(m => m.TraceId == _trace!.TraceId);
            Rows.Add(new TraceMeasurementModel(lastMeasurement));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Trace_statistics;
            IsOpen = true;
        }

        public void ShowReflectogram(int param)
        {
            if (SelectedRow == null) return;
            _reflectogramManager.SetTempFileName(TraceTitle, SelectedRow.Measurement.SorFileId, SelectedRow.Measurement.MeasurementTimestamp);
            if (param == 2)
                _reflectogramManager.ShowRefWithBase(SelectedRow.Measurement.SorFileId);
            else
                _reflectogramManager.ShowOnlyCurrentMeasurement(SelectedRow.Measurement.SorFileId);
        }

        public void SaveReflectogramAs(bool param)
        {
            if (SelectedRow == null) return;
            _reflectogramManager.SetTempFileName(TraceTitle, SelectedRow.Measurement.SorFileId, SelectedRow.Measurement.MeasurementTimestamp);
            _reflectogramManager.SaveReflectogramAs(SelectedRow.Measurement.SorFileId, param);
        }

        public void ShowBaseReflectogram()
        {
            if (SelectedBaseRef == null) return;
            // do not use localized base ref type!
            _reflectogramManager.SetTempFileName(TraceTitle, SelectedBaseRef.BaseRefType.ToString(), SelectedBaseRef.AssignedAt);
            _reflectogramManager.ShowBaseReflectogram(SelectedBaseRef.SorFileId);
        }

        public void SaveBaseReflectogramAs()
        {
            if (SelectedBaseRef == null) return;
            // do not use localized base ref type!
            _reflectogramManager.SetTempFileName(TraceTitle, SelectedBaseRef.BaseRefType.ToString(), SelectedBaseRef.AssignedAt);
            _reflectogramManager.SaveBaseReflectogramAs(SelectedBaseRef.SorFileId);
        }

        public void ShowRftsEvents()
        {
            if (SelectedRow == null) return;
            _reflectogramManager.ShowRftsEvents(SelectedRow.Measurement.SorFileId, TraceTitle);
        }

        public void ShowTraceState()
        {
            if (SelectedRow == null) return;
            var lastRow = Rows.Last(); // click on the Row , so Rows collection couldn't be empty
            var lastEvent = Rows.LastOrDefault(r => r.IsOpticalEvent);
            var isLastAccident = lastEvent == null || 
                                 (SelectedRow.Measurement.SorFileId >= lastEvent.Measurement.SorFileId 
                                  && SelectedRow.Measurement.TraceState != FiberState.Ok);
            _traceStateViewsManager.ShowTraceState(SelectedRow.Measurement, lastRow.Measurement.SorFileId == SelectedRow.Measurement.SorFileId, isLastAccident);
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new())
        {
            await Task.Delay(1);
            IsOpen = false;
            return true;
        }

        public async void Close()
        {
            await TryCloseAsync();
        }
    }
}
