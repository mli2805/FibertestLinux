using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Autofac;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public partial class MainMenuViewModel
    {
        public async void ShowUsersGuide()
        {
            var usersGuide = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"..\UserGuide\FIBERTEST20ClientUGru.pdf"));
            if (!File.Exists(usersGuide))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error,
                    new List<string> { Resources.SID_Cannot_find_file_with_user_s_guide_, "", usersGuide }, 0);
                await _windowManager.ShowDialogWithAssignedOwner(mb);
                return;
            }
            Process.Start(usersGuide);
        }

        public async void LaunchLicenseView()
        {
            var vm = _globalScope.Resolve<LicenseViewModel>();
            vm.Initialize();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchAboutView()
        {
            var vm = _globalScope.Resolve<AboutViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}