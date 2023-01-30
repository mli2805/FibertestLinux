using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class RtuStateViewsManager
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly Model _reaModel;
        private readonly CurrentUser _currentUser;
        private readonly ChildrenViews _childrenViews;
        private readonly RtuStateModelFactory _rtuStateModelFactory;
        private readonly TreeOfRtuModel _treeOfRtuModel;

        public Dictionary<Guid, RtuStateViewModel> LaunchedViews { get; set; } =
            new Dictionary<Guid, RtuStateViewModel>();

        public RtuStateViewsManager(ILifetimeScope globalScope, IWindowManager windowManager,
            Model reaModel, CurrentUser currentUser, ChildrenViews childrenViews,
            RtuStateModelFactory rtuStateModelFactory, TreeOfRtuModel treeOfRtuModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _reaModel = reaModel;
            _currentUser = currentUser;
            _childrenViews = childrenViews;
            _rtuStateModelFactory = rtuStateModelFactory;
            _treeOfRtuModel = treeOfRtuModel;

            childrenViews.PropertyChanged += ChildrenViews_PropertyChanged;
        }

        private void ChildrenViews_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ChildrenViews.ShouldBeClosed))
            {
                if (((ChildrenViews) sender).ShouldBeClosed)
                {
                    foreach (var pair in LaunchedViews.ToList())
                    {
                        pair.Value.TryCloseAsync();
                        LaunchedViews.Remove(pair.Key);
                    }
                }
            }
        }

        // user clicked on RtuLeaf
        public void ShowRtuState(RtuLeaf rtuLeaf)
        {
            Show(rtuLeaf, isUserAskedToOpenView: true, changes: RtuPartStateChanges.NoChanges);
        }

        public void Apply(object evnt)
        {
            switch (evnt)
            {
                case NetworkEventAdded e: NotifyUserRtuAvailabilityChanged(e); return;
                case BopNetworkEventAdded e: NotifyUserBopStateChanged(e); return;
                case MeasurementAdded e: NotifyUserMonitoringResult(e); return;
                case TraceAttached e: NotifyUserTraceChanged(e.TraceId); return;
                case TraceDetached e: NotifyUserTraceChanged(e.TraceId); return;
                case TraceUpdated e: NotifyUserTraceChanged(e.Id); return;
                case RtuUpdated e: NotifyUserRtuUpdated(e.RtuId); return;
                case RtuInitialized e: NotifyUserRtuUpdated(e.Id); return;
                case MonitoringSettingsChanged e:
                    if (e.IsMonitoringOn)
                        NotifyUserMonitoringStarted(e.RtuId);
                    else
                        NotifyUserMonitoringStopped(e.RtuId);
                    return;
                case MonitoringStarted e: 
                    NotifyUserMonitoringStarted(e.RtuId); 
                    return;
                case MonitoringStopped e: NotifyUserMonitoringStopped(e.RtuId); return;
                case ResponsibilitiesChanged _: ChangeResponsibilities(); return;
                default: return;
            }
        }

        // Server sent network event
        private void NotifyUserRtuAvailabilityChanged(NetworkEventAdded networkEventAdded)
        {
            var rtu = _reaModel.Rtus.FirstOrDefault(r => r.Id == networkEventAdded.RtuId);
            if (rtu == null || !rtu.ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(networkEventAdded.RtuId);
            if (rtuLeaf == null) return;
            if (LaunchedViews.TryGetValue(rtuLeaf.Id, out var vm))
                vm.RefreshModel(rtuLeaf);
        }

        // Server sent BOP network event
        private void NotifyUserBopStateChanged(BopNetworkEventAdded bopNetworkEventAdded)
        {
            var rtu = _reaModel.Rtus.FirstOrDefault(r => r.Id == bopNetworkEventAdded.RtuId);
            if (rtu == null || !rtu.ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(bopNetworkEventAdded.RtuId);
            if (LaunchedViews.TryGetValue(rtuLeaf.Id, out var vm))
                vm.RefreshModel(rtuLeaf);
        }

        public void NotifyUserRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            ClearClosedViews();
            if (LaunchedViews.TryGetValue(dto.RtuId, out var vm))
                vm.NotifyUserCurrentMonitoringStep(dto);
        }

        private void NotifyUserMonitoringResult(MeasurementAdded dto)
        {
            ClearClosedViews();
            if (LaunchedViews.TryGetValue(dto.RtuId, out var vm))
                vm.NotifyUserMonitoringResult(dto);
        }

        public void NotifyUserMonitoringStopped(Guid rtuId)
        {
            if (LaunchedViews.TryGetValue(rtuId, out var vm))
                vm.MonitoringStopped();
        }

        public void NotifyUserMonitoringStarted(Guid rtuId)
        {
            if (LaunchedViews.TryGetValue(rtuId, out var vm))
                vm.MonitoringStarted();
        }

        public void NotifyUserTraceChanged(Guid traceId)
        {
            var traceLeaf = _treeOfRtuModel.GetById(traceId);
            if (traceLeaf == null) return; // trace\RTU could be not in zone

            var rtuLeaf = (RtuLeaf)(traceLeaf.Parent is RtuLeaf ? traceLeaf.Parent : traceLeaf.Parent.Parent);
            if (LaunchedViews.TryGetValue(rtuLeaf.Id, out var vm))
                vm.RefreshModel(rtuLeaf);
        }

        public void NotifyUserRtuUpdated(Guid rtuId)
        {
            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(rtuId);
            if (rtuLeaf == null) return; // trace\RTU could be not in zone

            if (LaunchedViews.TryGetValue(rtuId, out var vm))
                vm.RefreshModel(rtuLeaf);
        }

        private void ClearClosedViews()
        {
            var closed = (from pair in LaunchedViews where !pair.Value.IsOpen select pair.Key).ToList();
            foreach (var view in closed)
            {
                LaunchedViews.Remove(view);
            }
        }

        private void Show(RtuLeaf rtuLeaf, bool isUserAskedToOpenView, RtuPartStateChanges changes)
        {
            ClearClosedViews();

            var rtuId = rtuLeaf.Id;

            if (LaunchedViews.TryGetValue(rtuId, out var vm))
            {
                vm.Close();
                LaunchedViews.Remove(rtuId);
            }


            vm = _globalScope.Resolve<RtuStateViewModel>();
            vm.Initialize(_rtuStateModelFactory.Create(rtuLeaf), isUserAskedToOpenView, changes);
            _windowManager.ShowWindowWithAssignedOwner(vm);

            LaunchedViews.Add(rtuId, vm);
            _childrenViews.ShouldBeClosed = false;
        }


        private void ChangeResponsibilities()
        {
            foreach (var pair in LaunchedViews)
            {
                var rtu = _reaModel.Rtus.First(r => r.Id == pair.Key);
                if (!rtu.ZoneIds.Contains(_currentUser.ZoneId))
                    pair.Value.Close();
            }
        }

    }
}