using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class OpticalEventsViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly RtuFilterViewModel _rtuFilterViewModel;
        private readonly IWindowManager _windowManager;

        public string TableTitle { get; set; } = null!;
        public ObservableCollection<OpticalEventModel> Rows { get; set; } = new();

        private OpticalEventModel? _selectedRow;
        public OpticalEventModel? SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public List<TraceStateFilter> TraceStateFilters { get; set; } = null!;

        private TraceStateFilter _selectedTraceStateFilter = null!;
        public TraceStateFilter SelectedTraceStateFilter
        {
            get => _selectedTraceStateFilter;
            set
            {
                if (Equals(value, _selectedTraceStateFilter)) return;
                _selectedTraceStateFilter = value;
                var view = CollectionViewSource.GetDefaultView(Rows);
                view.Refresh();
            }
        }

        private RtuGuidFilter _selectedRtuFilter = null!;
        public RtuGuidFilter SelectedRtuFilter
        {
            get => _selectedRtuFilter;
            set
            {
                _selectedRtuFilter = value;
                var view = CollectionViewSource.GetDefaultView(Rows);

                view.MoveCurrentToFirst();
                view.Refresh();

                SelectedRow = (OpticalEventModel)view.CurrentItem;
                RtuFilterNow = _selectedRtuFilter.ToString();
            }
        }

        public List<EventStatusFilter> EventStatusFilters { get; set; } = null!;

        private EventStatusFilter _selectedEventStatusFilter = null!;
        public EventStatusFilter SelectedEventStatusFilter
        {
            get => _selectedEventStatusFilter;
            set
            {
                if (Equals(value, _selectedEventStatusFilter)) return;
                _selectedEventStatusFilter = value;
                var view = CollectionViewSource.GetDefaultView(Rows);
                view.Refresh();
            }
        }

        private string _rtuFilterNow = Resources.SID__no_filter_;
        public string RtuFilterNow
        {
            get => _rtuFilterNow;
            set
            {
                if (value == _rtuFilterNow) return;
                _rtuFilterNow = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility ForDev { get; set; }

        public OpticalEventsViewModel(ILifetimeScope globalScope, Model readModel,
            ReflectogramManager reflectogramManager, TraceStateViewsManager traceStateViewsManager,
            RtuFilterViewModel rtuFilterViewModel, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _reflectogramManager = reflectogramManager;
            _traceStateViewsManager = traceStateViewsManager;
            _rtuFilterViewModel = rtuFilterViewModel;
            _windowManager = windowManager;

            InitializeTraceStateFilters();
            SelectedRtuFilter = new RtuGuidFilter();
            InitializeEventStatusFilters();
        }

        protected override void OnViewLoaded(object o)
        {
            var currentUser = _globalScope.Resolve<CurrentUser>();
            ForDev = currentUser.Role == Role.Developer ? Visibility.Visible : Visibility.Collapsed;
            var view = CollectionViewSource.GetDefaultView(Rows);
            if (view == null) // cancel was pressed instead of login
                return;
            view.Filter += OnFilter;
            view.SortDescriptions.Add(new SortDescription(@"Nomer", ListSortDirection.Descending));
        }

        private void InitializeTraceStateFilters()
        {
            TraceStateFilters = new List<TraceStateFilter>
            {
                new(),
                new(FiberState.Ok),
                new(FiberState.Minor),
                new(FiberState.Major),
                new(FiberState.Critical),
                new(FiberState.FiberBreak),
                new(FiberState.NoFiber),
                new(FiberState.User)
            };

            SelectedTraceStateFilter = TraceStateFilters.First();
        }

        private void InitializeEventStatusFilters()
        {
            EventStatusFilters = new List<EventStatusFilter>() { new EventStatusFilter() };
            foreach (var eventStatus in EventStatusExt.EventStatusesInRightOrder)
                EventStatusFilters.Add(new EventStatusFilter(eventStatus));
            SelectedEventStatusFilter = EventStatusFilters.First();
        }

        private bool OnFilter(object o)
        {
            var opticalEventVm = (OpticalEventModel)o;
            return 
                // STATE filter
                (SelectedTraceStateFilter.IsOn == false || 
                 (SelectedTraceStateFilter.TraceState == opticalEventVm.TraceState 
                    && opticalEventVm.BaseRefType != BaseRefType.Fast)) 
                // STATUS filter
                   && (SelectedEventStatusFilter.IsOn == false || 
                       SelectedEventStatusFilter.EventStatus == opticalEventVm.EventStatus)
                // RTU filter
                   && (SelectedRtuFilter.IsOn == false || SelectedRtuFilter.RtuId == opticalEventVm.RtuId);
        }

        public void RefreshRowsWithUpdatedRtu(Guid rtuId)
        {
            foreach (var opticalEventModel in Rows.Where(m => m.RtuId == rtuId).ToList())
            {
                Rows.Remove(opticalEventModel);
                opticalEventModel.RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuId)?.Title ?? "";
                Rows.Add(opticalEventModel);
            }
        }

        public void RefreshRowsWithUpdatedTrace(Guid traceId)
        {
            foreach (var opticalEventModel in Rows.Where(m => m.TraceId == traceId).ToList())
            {
                Rows.Remove(opticalEventModel);
                opticalEventModel.TraceTitle = _readModel.Traces.FirstOrDefault(t => t.TraceId == traceId)?.Title ?? "";
                Rows.Add(opticalEventModel);
            }
        }


        public void AddEvent(Measurement measurement)
        {
            Rows.Add(new OpticalEventModel()
            {
                Nomer = measurement.SorFileId,
                MeasurementTimestamp = measurement.MeasurementTimestamp,
                EventRegistrationTimestamp = measurement.EventRegistrationTimestamp,
                RtuId = measurement.RtuId,
                RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == measurement.RtuId)?.Title ?? "",
                TraceId = measurement.TraceId,
                TraceTitle = _readModel.Traces.FirstOrDefault(t => t.TraceId == measurement.TraceId)?.Title ?? "",
                BaseRefType = measurement.BaseRefType,
                TraceState = measurement.TraceState,

                EventStatus = measurement.EventStatus,
                StatusChangedTimestamp = measurement.EventStatus.IsStatusAssignedByUser()
                    ? measurement.StatusChangedTimestamp.ToString(Thread.CurrentThread.CurrentUICulture)
                    : "",
                StatusChangedByUser = measurement.EventStatus.IsStatusAssignedByUser()
                    ? measurement.StatusChangedByUser!
                    : "",

                Comment = measurement.Comment,
                Accidents = measurement.Accidents,
                SorFileId = measurement.SorFileId,
            });
        }

        public void RemoveEventsOfRtu(Guid rtuId)
        {
            foreach (var opticalEventModel in Rows.Where(m => m.RtuId == rtuId).ToList())
            {
                Rows.Remove(opticalEventModel);
            }
        }

        public void RemoveEventsOfTrace(Guid traceId)
        {
            for (var i = Rows.Count - 1; i >= 0; i--)
            {
                if (Rows[i].TraceId == traceId)
                    Rows.RemoveAt(i);
            }
        }

        public void UpdateEvent(MeasurementUpdated dto)
        {
            var opticalEventModel = Rows.FirstOrDefault(l => l.SorFileId == dto.SorFileId);
            if (opticalEventModel == null) return;


            Rows.Remove(opticalEventModel);

            if (opticalEventModel.EventStatus != dto.EventStatus)
            {
                opticalEventModel.EventStatus = dto.EventStatus;
                opticalEventModel.StatusChangedByUser = dto.StatusChangedByUser ?? "";
                opticalEventModel.StatusChangedTimestamp = 
                    dto.StatusChangedTimestamp.ToString(CultureInfo.CurrentCulture);
            }
            opticalEventModel.Comment = dto.Comment;

            Rows.Add(opticalEventModel);
        }

        public void ShowReflectogram(int param)
        {
            _reflectogramManager.SetTempFileName(SelectedRow!.TraceTitle, 
                SelectedRow.Nomer, SelectedRow.EventRegistrationTimestamp);
            if (param == 2)
                _reflectogramManager.ShowRefWithBase(SelectedRow.SorFileId);
            else
                _reflectogramManager.ShowOnlyCurrentMeasurement(SelectedRow.SorFileId);
        }

        public void SaveReflectogramAs(bool shouldBaseRefBeExcluded)
        {
            _reflectogramManager.SetTempFileName(SelectedRow!.TraceTitle, 
                SelectedRow.SorFileId, SelectedRow.EventRegistrationTimestamp);
            _reflectogramManager.SaveReflectogramAs(SelectedRow.SorFileId, shouldBaseRefBeExcluded);
        }

        public void ShowRftsEvents()
        {
            _reflectogramManager.ShowRftsEvents(SelectedRow!.SorFileId, SelectedRow.TraceTitle);
        }

        public void ShowTraceState()
        {
            var lastMeasurement = _readModel.Measurements.LastOrDefault(m => m.TraceId == SelectedRow!.TraceId);
            var isLastMeasurement = lastMeasurement == null || lastMeasurement.SorFileId == SelectedRow!.SorFileId;

            var lastEvent = _readModel.Measurements.LastOrDefault(m => m.TraceId == SelectedRow!.TraceId
                                                               && m.EventStatus > EventStatus.JustMeasurementNotAnEvent);
            var isLastAccident = lastEvent == null || lastEvent.SorFileId <= SelectedRow!.SorFileId;
            _traceStateViewsManager.ShowTraceState(SelectedRow!, isLastMeasurement, isLastAccident);
        }

        public async void RecalculateAccidents()
        {
            byte[] sorBytes = await _reflectogramManager.GetSorBytes(SelectedRow!.SorFileId);
            var sorData = SorData.FromBytes(sorBytes);
            var _ =
                _globalScope.Resolve<AccidentsFromSorExtractor>()
                    .GetAccidents(sorData, SelectedRow.TraceId, true);
        }

        public async void ShowRtuFilter()
        {
            _rtuFilterViewModel.Initialize();
            var modalResult = await _windowManager.ShowDialogWithAssignedOwner(_rtuFilterViewModel);
            if (modalResult == true)
            {
                SelectedRtuFilter = _rtuFilterViewModel.SelectedRow;
            }
        }

        public void RemoveEventsAndSors(EventsAndSorsRemoved evnt)
        {
            if (!evnt.IsOpticalEvents) return;

            foreach (var opticalEventModel in Rows.ToList())
            {
                if (_readModel.Measurements.All(m => m.SorFileId != opticalEventModel.SorFileId))
                    Rows.Remove(opticalEventModel);
            }
        }
    }
}
