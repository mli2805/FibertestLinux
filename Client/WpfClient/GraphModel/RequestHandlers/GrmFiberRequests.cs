using System;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class GrmFiberRequests
    {
        private readonly ILifetimeScope _globalScope;
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly Model _model;
        private readonly IWindowManager _windowManager;


        public GrmFiberRequests(ILifetimeScope globalScope, GrpcC2DRequests grpcC2DRequests, Model model, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _grpcC2DRequests = grpcC2DRequests;
            _model = model;
            _windowManager = windowManager;
        }

        public async Task AddFiber(AddFiber cmd)
        {
            if (! await Validate(cmd)) return;
            cmd.FiberId = Guid.NewGuid();
            await _grpcC2DRequests.SendEventSourcingCommand(cmd);
        }

        private async Task<bool> Validate(AddFiber cmd)
        {
            if (cmd.NodeId1 == cmd.NodeId2)
                return false;

            if (!_model.HasDirectFiberDontMindPoints(cmd.NodeId1, cmd.NodeId2))
                return true;
            await _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Section_already_exists));
            return false;
        }

        public async Task UpdateFiber(RequestUpdateFiber request)
        {
            var cmd = await PrepareCommand(request);
            if (cmd == null) return;
            await _grpcC2DRequests.SendEventSourcingCommand(cmd);
        }

        private async Task<UpdateFiber> PrepareCommand(RequestUpdateFiber request)
        {
            var vm = _globalScope.Resolve<FiberUpdateViewModel>();
            await vm.Initialize(request.Id);
            await _windowManager.ShowDialogWithAssignedOwner(vm);

            return vm.Command;
        }

        public async Task RemoveFiber(RemoveFiber cmd)
        {
            await _grpcC2DRequests.SendEventSourcingCommand(cmd);
        }
    }
}