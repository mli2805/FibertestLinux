using Autofac;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public partial class MainMenuViewModel
    {
        public async void LaunchGisSettingsView()
        {
            var vm = _globalScope.Resolve<GisSettingsViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchGraphVisibilitySettingsView()
        {
            var vm = _globalScope.Resolve<GraphVisibilitySettingsViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchSmtpSettingsView()
        {
            var vm = _globalScope.Resolve<SmtpSettingsViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchSmsSettingsView()
        {
            var vm = _globalScope.Resolve<SmsSettingsViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchSnmpSettingsView()
        {
            var vm = _globalScope.Resolve<SnmpSettingsViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchClientSettingsView()
        {
            var vm = _globalScope.Resolve<ConfigurationViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}