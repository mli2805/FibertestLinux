using System.Threading.Tasks;
using Autofac;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public partial class MainMenuViewModel
    {
        public async Task LaunchCleaningView()
        {
            var vm = _globalScope.Resolve<DbOptimizationViewModel>();
            await vm.Initialize();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async Task LaunchGraphOptimizationView()
        {
            var vm = _globalScope.Resolve<GraphOptimizationViewModel>();
            await vm.Initialize();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchTcesView()
        {
            _tcesViewModel.Initialize();
            await _windowManager.ShowWindowWithAssignedOwner(_tcesViewModel);
        }
    }
}
