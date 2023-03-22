using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class LandmarksViewsManager
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly ChildrenViews _childrenViews;
        private readonly Model _readModel;
        private readonly TraceChoiceViewModel _traceChoiceViewModel;
        private List<LandmarksViewModel> LaunchedViews { get; } = new List<LandmarksViewModel>();

        public LandmarksViewsManager(ILifetimeScope globalScope, IWindowManager windowManager, 
            ChildrenViews childrenViews, Model readModel, TraceChoiceViewModel traceChoiceViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _childrenViews = childrenViews;
            _readModel = readModel;
            _traceChoiceViewModel = traceChoiceViewModel;

            childrenViews.PropertyChanged += ChildrenViews_PropertyChanged;
        }

        private void ChildrenViews_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ChildrenViews.ShouldBeClosed))
            {
                if (((ChildrenViews) sender!).ShouldBeClosed)
                {
                    foreach (var traceStateViewModel in LaunchedViews.ToArray())
                    {
                        traceStateViewModel.TryCloseAsync();
                        LaunchedViews.Remove(traceStateViewModel);
                    }
                }
            }
        }

        public async Task<int> InitializeFromRtu(Guid rtuId)
        {
            var vm = _globalScope.Resolve<LandmarksViewModel>();
            var res = await vm.InitializeFromRtu(rtuId);
            LaunchedViews.Add(vm);
            _childrenViews.ShouldBeClosed = false;
            await _windowManager.ShowWindowWithAssignedOwner(vm);
            return res;
        }

        public async Task<int> InitializeFromTrace(Guid traceId, Guid selectedNodeId)
        {
            var vm = _globalScope.Resolve<LandmarksViewModel>();
            var res = await vm.InitializeFromTrace(traceId, selectedNodeId);
            LaunchedViews.Add(vm);
            _childrenViews.ShouldBeClosed = false;
            await _windowManager.ShowWindowWithAssignedOwner(vm);
            return res;
        }

        public async Task<int> InitializeFromNode(Guid nodeId)
        {
            var traces = _readModel.Traces.Where(t => t.NodeIds.Contains(nodeId)).ToList();
            if (traces.Count == 0) return -1;
            if (traces.Count == 1)
                return await InitializeFromTrace(traces.First().TraceId, nodeId);

            _traceChoiceViewModel.Initialize(traces);
            await _windowManager.ShowDialogWithAssignedOwner(_traceChoiceViewModel);
            if (!_traceChoiceViewModel.IsAnswerPositive)
                return -1;
            var traceId = _traceChoiceViewModel.SelectedTrace.TraceId;
            return await InitializeFromTrace(traceId, nodeId);

//            var vm = _globalScope.Resolve<LandmarksViewModel>();
//            var res = await vm.InitializeFromNode(nodeId);
//            if (vm.SelectedTrace == null) return -1;
//            LaunchedViews.Add(vm);
//            _windowManager.ShowWindowWithAssignedOwner(vm);
//            return res;
        }

        public async Task Apply(object e)
        {
            switch (e)
            {
                case RtuUpdated _: 
                case EquipmentUpdated _:
                case EquipmentIntoTraceIncluded _:
                case EquipmentFromTraceExcluded _:
                case NodeUpdatedAndMoved _:
                case NodeUpdated _: 
                case NodeMoved _: 
                    foreach (var v in LaunchedViews) 
                        await v.RefreshAsChangesReaction(); 
                    return; }
        }

    }
}
