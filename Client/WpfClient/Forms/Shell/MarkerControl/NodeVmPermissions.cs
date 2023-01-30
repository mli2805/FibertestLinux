using System.Linq;
using Autofac;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public class NodeVmPermissions
    {
        private readonly CurrentUser _currentUser;
        private readonly ILifetimeScope _globalScope;

        public NodeVmPermissions(CurrentUser currentUser, ILifetimeScope globalScope)
        {
            _currentUser = currentUser;
            _globalScope = globalScope;
        }

        public bool CanUpdateNode(object parameter)
        {
            if (parameter == null) return false;
            var marker = (MarkerControl) parameter;
            return marker.Type != EquipmentType.AdjustmentPoint;
        }

        public bool CanAddEquipment(object parameter)
        {
            return HasPrevilegesAndNotAdjustmentPoint(parameter);
        }

        public bool CanLandmarks(object parameter)
        {
            if (parameter == null) return false;
            var marker = (MarkerControl) parameter;
            return marker.Type != EquipmentType.AdjustmentPoint;
        }

        public bool CanRemoveNode(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var marker = (MarkerControl)parameter;
            if (marker.Type == EquipmentType.AdjustmentPoint) return true;
            if (marker.Owner.GraphReadModel.ReadModel.Traces.Any(t => t.NodeIds.Contains(marker.GMapMarker.Id) && t.HasAnyBaseRef)) return false;

            var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
            if (vm.IsOpen && vm.IsNodeUsed(marker.GMapMarker.Id)) return false;

            return marker.Owner.GraphReadModel.ReadModel.Traces.All(t => t.NodeIds.Last() != marker.GMapMarker.Id);
        }

        public bool CanStartAddFiber(object parameter)
        {
            return HasPrevilegesAndNotAdjustmentPoint(parameter);
        }

        public bool CanStartAddFiberWithNodes(object parameter)
        {
            return HasPrevilegesAndNotAdjustmentPoint(parameter);
        }

        private bool HasPrevilegesAndNotAdjustmentPoint(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var marker = (MarkerControl) parameter;
            return marker.Type != EquipmentType.AdjustmentPoint;
        }
    }
}