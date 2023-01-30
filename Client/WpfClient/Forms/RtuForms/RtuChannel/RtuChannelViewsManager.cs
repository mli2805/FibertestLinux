using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class RtuChannelViewsManager
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly ChildrenViews _childrenViews;

        public Dictionary<Guid, RtuChannelViewModel> LaunchedViews { get; set; } =
            new Dictionary<Guid, RtuChannelViewModel>();

        public RtuChannelViewsManager(ILifetimeScope globalScope, IWindowManager windowManager, 
            Model readModel, CurrentUser currentUser, ChildrenViews childrenViews)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _readModel = readModel;
            _currentUser = currentUser;
            _childrenViews = childrenViews;
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

        public void Apply(object evnt)
        {
            switch (evnt)
            {
                case NetworkEventAdded e: NotifyUserRtuChannelStateChanged(e); return;
                case ResponsibilitiesChanged _: ChangeResponsibilities(); return;
                default: return;
            }
        }

        private void NotifyUserRtuChannelStateChanged(NetworkEventAdded networkEventAdded)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == networkEventAdded.RtuId);
            if (rtu == null || !rtu.ZoneIds.Contains(_currentUser.ZoneId)) return;

            Show(networkEventAdded);
        }

        private void ChangeResponsibilities()
        {
            foreach (var pair in LaunchedViews)
            {
                var rtu = _readModel.Rtus.First(r => r.Id == pair.Key);
                if (!rtu.ZoneIds.Contains(_currentUser.ZoneId))
                    pair.Value.TryCloseAsync();
            }
        }

        private void Show(NetworkEventAdded networkEventAdded)
        {
            ClearClosedViews();

            var vm = LaunchedViews.FirstOrDefault(m => m.Key == networkEventAdded.RtuId).Value;
            if (vm != null)
            {
                vm.TryCloseAsync();
                LaunchedViews.Remove(networkEventAdded.RtuId);
            }

            vm = _globalScope.Resolve<RtuChannelViewModel>();
            vm.Initialize(networkEventAdded);
            _windowManager.ShowWindowWithAssignedOwner(vm);

            LaunchedViews.Add(networkEventAdded.RtuId, vm);
            _childrenViews.ShouldBeClosed = false;
        }

        private void ClearClosedViews()
        {
            var closed = (from pair in LaunchedViews where !pair.Value.IsOpen select pair.Key).ToList();
            foreach (var view in closed)
            {
                LaunchedViews.Remove(view);
            }
        }

    }
}
