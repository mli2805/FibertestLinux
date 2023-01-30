using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class AddEquipmentIntoNodeBuilder
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _model;
        private readonly IWindowManager _windowManager;

        public AddEquipmentIntoNodeBuilder(ILifetimeScope globalScope, Model model, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _model = model;
            _windowManager = windowManager;
        }

        public async Task<AddEquipmentIntoNode?> BuildCommand(Guid nodeId)
        {
            var tracesInNode = _model.Traces.Where(t => t.NodeIds.Contains(nodeId)).ToList();
            TracesToEquipmentInjectionViewModel tracesToEquipmentInjectionVm = null;
            if (tracesInNode.Count > 0)
            {
                tracesToEquipmentInjectionVm = new TracesToEquipmentInjectionViewModel(tracesInNode);
                await _windowManager.ShowDialogWithAssignedOwner(tracesToEquipmentInjectionVm);
                if (!tracesToEquipmentInjectionVm.ShouldWeContinue)
                    return null;
            }

            var vm = _globalScope.Resolve<EquipmentInfoViewModel>();
            vm.InitializeForAdd(nodeId);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            if (vm.Command == null)
                return null;
            var command = (AddEquipmentIntoNode)vm.Command;


            if (tracesToEquipmentInjectionVm != null)
                command.TracesForInsertion = tracesToEquipmentInjectionVm.GetChosenTraces();
            return command;
        }
    }


}
