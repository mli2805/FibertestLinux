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
    public class LicenseSender
    {
        private readonly ILifetimeScope _globalScope;
        private readonly LicenseCommandFactory _licenseCommandFactory;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly ILicenseFileChooser _licenseFileChooser;
        private readonly LicenseFromFileDecoder _licenseFromFileDecoder;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;

        private SecurityAdminConfirmationViewModel _vm;

        public string SecurityAdminPassword;

        public LicenseSender(ILifetimeScope globalScope, IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager,
            LicenseCommandFactory licenseCommandFactory, ILicenseFileChooser licenseFileChooser,
            LicenseFromFileDecoder licenseFromFileDecoder, CurrentUser currentUser)
        {
            _globalScope = globalScope;
            _licenseCommandFactory = licenseCommandFactory;
            _c2DWcfManager = c2DWcfManager;
            _licenseFileChooser = licenseFileChooser;
            _licenseFromFileDecoder = licenseFromFileDecoder;
            _windowManager = windowManager;
            _currentUser = currentUser;
        }

        public async Task<bool> ApplyLicenseFromFile(string initialDirectory = "")
        {
            var filename = _licenseFileChooser.ChooseFilename(initialDirectory);
            if (filename == null)
                return false;

            var licenseInFile = await _licenseFromFileDecoder.Decode(filename);
            if (licenseInFile == null)
                return false;

            return await ApplyLicenseFromFile(licenseInFile);
        }

        private async Task<bool> ApplyLicenseFromFile(LicenseInFile licenseInFile)
        {
            var cmd = _licenseCommandFactory.CreateFromFile(licenseInFile, _currentUser.UserId);
            if (licenseInFile.IsMachineKeyRequired)
            {
                if (!IsCorrectPasswordEntered(licenseInFile))
                    return false;
            }

            return await SendApplyLicenseCommand(cmd);
        }

        public async Task<bool> ApplyDemoLicense()
        {
            var cmd = _licenseCommandFactory.CreateDemo();
            return await SendApplyLicenseCommand(cmd);
        }

        private async Task<bool> SendApplyLicenseCommand(ApplyLicense cmd)
        {
            string result;

            using (_globalScope.Resolve<IWaitCursor>())
            {
                result = await _c2DWcfManager.SendCommandAsObj(cmd);
            }

            var vm = result != null
                ? new MyMessageBoxViewModel(MessageType.Error, ((ReturnCode)int.Parse(result)).GetLocalizedString())
                : new MyMessageBoxViewModel(MessageType.Information, Resources.SID_License_applied_successfully_);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            return result == null;
        }

        private bool IsCorrectPasswordEntered(LicenseInFile licenseInFile)
        {
            _vm = new SecurityAdminConfirmationViewModel();
            _vm.Initialize(licenseInFile.Lk());
            _windowManager.ShowDialogWithAssignedOwner(_vm);
            if (_vm.IsOkPressed)
            {
                var password = (string)Cryptography.Decode(licenseInFile.SecurityAdminPassword);
                if (_vm.PasswordViewModel.Password != password)
                {
                    var strs = new List<string>() { Resources.SID_Wrong_password, "", Resources.SID_License_will_not_be_applied_ };
                    var mb = new MyMessageBoxViewModel(MessageType.Information, strs, 0);
                    _windowManager.ShowDialogWithAssignedOwner(mb);
                    return false;
                }
            }
            else
            {
                var strs = new List<string>() { Resources.SID_Password_is_not_entered_, "", Resources.SID_License_will_not_be_applied_ };
                var mb = new MyMessageBoxViewModel(MessageType.Information, strs, 0);
                _windowManager.ShowDialogWithAssignedOwner(mb);
                return false;
            }

            SecurityAdminPassword = _vm.PasswordViewModel.Password;
            return true;
        }
    }
}