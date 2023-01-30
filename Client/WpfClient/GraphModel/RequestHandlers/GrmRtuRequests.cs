using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class GrmRtuRequests
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _model;
        private readonly CurrentGis _currentGis;
        private readonly IWindowManager _windowManager;
        private readonly RtuRemover _rtuRemover;
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;

        public GrmRtuRequests(ILifetimeScope globalScope, Model model, CurrentGis currentGis,
           IWindowManager windowManager, RtuRemover rtuRemover, CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
            _model = model;
            _currentGis = currentGis;
            _windowManager = windowManager;
            _rtuRemover = rtuRemover;
            _commonStatusBarViewModel = commonStatusBarViewModel;
        }

        public async void AddRtuAtGpsLocation(RequestAddRtuAtGpsLocation request)
        {
            var vm = _globalScope.Resolve<RtuUpdateViewModel>();
            vm.Initialize(request);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void UpdateRtu(RequestUpdateRtu request)
        {
            var rtu = _model.Rtus.First(r => r.NodeId == request.NodeId);
            var vm = _globalScope.Resolve<RtuUpdateViewModel>();
            vm.Initialize(rtu.Id);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async Task<string> RemoveRtu(RequestRemoveRtu request)
        {
            var rtu = _model.Rtus.First(r => r.NodeId == request.NodeId);
            return await _rtuRemover.Fire(rtu);
        }


        public async void DefineTraceStepByStep(Guid rtuNodeId, string rtuTitle)
        {
            var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
            if (vm.IsOpen)
            {
                var vm1 = new MyMessageBoxViewModel(MessageType.Information, Resources.SID_Step_by_step_trace_definition_is_started_already_);
                await _windowManager.ShowDialogWithAssignedOwner(vm1);
                return;
            }

            _commonStatusBarViewModel.StatusBarMessage2 = string.Format(Resources.SID_Trace_definition_mode__Minimum_zoom__0_, _currentGis.ThresholdZoom);
            using (_globalScope.Resolve<IWaitCursor>())
            {
                var unused = await vm.Initialize(rtuNodeId, rtuTitle);
            }
            await _windowManager.ShowWindowWithAssignedOwner(vm);
        }
    }
}