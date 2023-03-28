using Autofac;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public partial class MainMenuViewModel
    {
        public async void LaunchResponsibilityZonesView()
        {
            var vm = _globalScope.Resolve<ZonesViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchUserListView()
        {
            var vm = _globalScope.Resolve<UserListViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchObjectsToZonesView()
        {
            var vm = _globalScope.Resolve<ObjectsAsTreeToZonesViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

    }
}
