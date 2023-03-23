using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using GMap.NET;

namespace Fibertest.WpfClient
{
    public class EquipmentEventsOnGraphExecutor
    {
        private readonly ILifetimeScope _globalScope;
        private readonly GraphReadModel _model;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly IWindowManager _windowManager;

        public EquipmentEventsOnGraphExecutor(ILifetimeScope globalScope, GraphReadModel model,
            Model readModel, CurrentUser currentUser, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _model = model;
            _readModel = readModel;
            _currentUser = currentUser;
            _windowManager = windowManager;
        }

        public void AddEquipmentAtGpsLocation(EquipmentAtGpsLocationAdded evnt)
        {
            if (_currentUser.Role > Role.Root) return;

            var nodeVm = new NodeVm()
            {
                Id = evnt.NodeId,
                State = FiberState.Ok,
                Type = evnt.Type,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude)
            };
            _model.Data.Nodes.Add(nodeVm);
        }

        public void AddEquipmentAtGpsLocationWithNodeTitle(EquipmentAtGpsLocationWithNodeTitleAdded evnt)
        {
            if (_currentUser.Role > Role.Root) return;

            var nodeVm = new NodeVm()
            {
                Id = evnt.NodeId,
                State = FiberState.Ok,
                Type = evnt.Type,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude),
                Title = evnt.Title,
            };
            _model.Data.Nodes.Add(nodeVm);
        }

        public void AddEquipmentIntoNode(EquipmentIntoNodeAdded evnt)
        {
            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == evnt.NodeId);
            if (nodeVm != null)
                nodeVm.Type = evnt.Type;
        }

        public async Task UpdateEquipment(EquipmentUpdated evnt)
        {
            var equipment = _readModel.Equipments.FirstOrDefault(e => e.EquipmentId == evnt.EquipmentId);
            if (equipment == null)
            {
                _model.Logger.Error(Logs.Client,$@"UpdateEquipment: equipment {evnt.EquipmentId.First6()} not found");
                if (_currentUser.Role <= Role.Root)
                    await _windowManager.ShowDialogWithAssignedOwner($@"не найдено редактируемое оборудование");
                return;
            }

            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == equipment.NodeId);
            if (nodeVm != null)
            {
                nodeVm.Type = evnt.Type;

                var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
                if (vm.IsOpen)
                    vm.UpdateNode(nodeVm.Id);
            }

        }

        public void RemoveEquipment(EquipmentRemoved evnt)
        {
            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == evnt.NodeId);
            if (nodeVm == null) return;
            var majorEquipmentInNode = _readModel.Equipments.Last(e => e.NodeId == nodeVm.Id).Type;
            nodeVm.Type = majorEquipmentInNode;
    
            var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
            if (vm.IsOpen)
                vm.UpdateNode(evnt.NodeId);
        }

    }
}