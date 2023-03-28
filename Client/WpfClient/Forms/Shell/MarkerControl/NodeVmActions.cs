using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class NodeVmActions
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;

        public NodeVmActions(ILifetimeScope globalScope, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
        }

        public async Task AskUpdateNode(object parameter)
        {
            await Task.Delay(0);
            var marker = (MarkerControl)parameter;
            var vm = _globalScope.Resolve<NodeUpdateViewModel>();
            vm.Initialize(marker.GMapMarker.Id);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }
        public async Task AskAddEquipment(object parameter)
        {
            var marker = (MarkerControl)parameter;
            await marker.Owner.GraphReadModel.GrmEquipmentRequests.
                AddEquipmentIntoNode(new RequestAddEquipmentIntoNode()
                    { NodeId = marker.GMapMarker.Id, IsCableReserveRequested = false });
        }

        public async Task AskLandmarks(object parameter)
        {
            var marker = (MarkerControl)parameter;
            var landmarksViewsManager = _globalScope.Resolve<LandmarksViewsManager>();
            await landmarksViewsManager.InitializeFromNode(marker.GMapMarker.Id);
        }

        public async Task AskRemoveNode(object parameter)
        {
            var marker = (MarkerControl)parameter;
            await marker.Owner.GraphReadModel.GrmNodeRequests.RemoveNode(marker.GMapMarker.Id, marker.Type);
        }
    }
}
