using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class TraceStatisticsViewsManager
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly ChildrenViews _childrenViews;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private Dictionary<Guid, TraceStatisticsViewModel> LaunchedViews { get; } = new Dictionary<Guid, TraceStatisticsViewModel>();

        public TraceStatisticsViewsManager(ILifetimeScope globalScope, IWindowManager windowManager, 
            ChildrenViews childrenViews, Model readModel, CurrentUser currentUser)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _childrenViews = childrenViews;
            _readModel = readModel;
            _currentUser = currentUser;

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

        public void Apply(object e)
        {
            switch (e)
            {
                case MeasurementAdded evnt: AddMeasurement(evnt); return;
                case TraceUpdated evnt: UpdateTrace(evnt); return;
                case RtuUpdated evnt: UpdateRtu(evnt); return;
                case ResponsibilitiesChanged evnt: ChangeResponsibility(evnt); return;
            }
        }

        private void ClearClosedViews()
        {
            var closed = (from pair in LaunchedViews where !pair.Value.IsOpen select pair.Key).ToList();
            foreach (var view in closed)
            {
                LaunchedViews.Remove(view);
            }
        }

        public void Show(Guid traceId)
        {
            ClearClosedViews();
            if (LaunchedViews.TryGetValue(traceId, out var vm))
            {
                vm.TryCloseAsync();
                LaunchedViews.Remove(traceId);
            }

            vm = _globalScope.Resolve<TraceStatisticsViewModel>();
            vm.Initialize(traceId);
            _windowManager.ShowWindowWithAssignedOwner(vm);

            LaunchedViews.Add(traceId, vm);
            _childrenViews.ShouldBeClosed = false;
        }

        private void AddMeasurement(MeasurementAdded evnt)
        {
            ClearClosedViews();
            var traceId = evnt.TraceId;

            if (LaunchedViews.TryGetValue(traceId, out var vm))
                vm.AddNewMeasurement();
        }

        private void UpdateTrace(TraceUpdated evnt)
        {
            if (LaunchedViews.TryGetValue(evnt.Id, out var vm))
                vm.TraceTitle = evnt.Title;
        }
        private void UpdateRtu(RtuUpdated evnt)
        {
            foreach (var pair in LaunchedViews)
            {
                var trace = _readModel.Traces.First(t => t.TraceId == pair.Key);
                if (trace.RtuId == evnt.RtuId)
                    pair.Value.RtuTitle = evnt.Title;
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void ChangeResponsibility(ResponsibilitiesChanged evnt)
        {
            foreach (var pair in LaunchedViews)
            {
                var trace = _readModel.Traces.First(t => t.TraceId == pair.Key);
                if (!trace.ZoneIds.Contains(_currentUser.ZoneId))
                    pair.Value.Close();
            }
        }
    }
}