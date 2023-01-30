using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class GraphOptimizationViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public string NodeCountStr { get; set; }
        public string FiberCountStr { get; set; }

        public bool IsEnabled { get; set; }

        public GraphOptimizationViewModel(ILifetimeScope globalScope, CurrentUser currentUser, Model readModel,
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            IsEnabled = currentUser.Role <= Role.Root;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Graph_of_traces_optimization;
        }

        public async Task Initialize()
        {
            var unusedElements = await _readModel.GetUnused();

            NodeCountStr = string.Format(Resources.SID__0__unused_nodes_found, unusedElements.Item1.Count);
            FiberCountStr = string.Format(Resources.SID__0__unused_fibers_found, unusedElements.Item2.Count);
        }

        public async Task Remove()
        {
            var vm2 = new MyMessageBoxViewModel(MessageType.Confirmation,
                new List<string>
                {
                    Resources.SID_Attention_, "",
                    Resources.SID_If_you_click_OK_now__the_data_will_be_permanently_deleted,
                    Resources.SID_with_no_possibility_to_restore_them_,
                }, 0);
            _windowManager.ShowDialogWithAssignedOwner(vm2);
            if (!vm2.IsAnswerPositive) return;

            string result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                result = await _c2DWcfManager.SendCommandAsObj(new RemoveUnused());
            }

            var vm = !string.IsNullOrEmpty(result)
                ? new MyMessageBoxViewModel(MessageType.Error, string.Format(Resources.SID_Graph_of_traces_optimization_failed___0_, result))
                : new MyMessageBoxViewModel(MessageType.Information, Resources.SID_Successfully_optimized_graph_of_traces_);
            await _windowManager.ShowDialogWithAssignedOwner(vm);

            await TryCloseAsync();
        }

        public async void Cancel()
        {
            await TryCloseAsync();
        }
    }
}
