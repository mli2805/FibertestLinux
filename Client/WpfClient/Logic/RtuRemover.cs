using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class RtuRemover
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly GrpcC2DRequests _grpcC2DRequests;

        public RtuRemover(ILifetimeScope globalScope, IWindowManager windowManager, GrpcC2DRequests grpcC2DRequests)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _grpcC2DRequests = grpcC2DRequests;
        }

        public async Task Fire(Rtu rtu)
        {
            if (!await _globalScope.Resolve<IRtuHolder>().SetRtuOccupationState(rtu.Id, rtu.Title, RtuOccupation.RemoveRtu))
                return;

            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, string.Format(Resources.SID_Remove_RTU___0____, rtu.Title));
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            if (!vm.IsAnswerPositive) return;
            var cmd = new RemoveRtu() { RtuId = rtu.Id, RtuNodeId = rtu.NodeId };
            await _grpcC2DRequests.SendEventSourcingCommand(cmd);
        }
    }
}