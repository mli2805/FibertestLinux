using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class LicenseSender
    {
        private readonly ILifetimeScope _globalScope;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly LicenseCommandFactory _licenseCommandFactory;
        private readonly ILicenseFileChooser _licenseFileChooser;
        private readonly LicenseFromFileDecoder _licenseFromFileDecoder;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;


        public string? SecurityAdminPassword;

        public LicenseSender(ILifetimeScope globalScope, GrpcC2DService grpcC2DService, IWindowManager windowManager,
            LicenseCommandFactory licenseCommandFactory, ILicenseFileChooser licenseFileChooser,
            LicenseFromFileDecoder licenseFromFileDecoder, CurrentUser currentUser)
        {
            _globalScope = globalScope;
            _grpcC2DService = grpcC2DService;
            _licenseCommandFactory = licenseCommandFactory;
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
            var cmd = _licenseCommandFactory.CreateFromFile(licenseInFile, _currentUser.UserId, _currentUser.UserName);
            if (licenseInFile.IsMachineKeyRequired)
            {
                if (! await IsCorrectPasswordEntered(licenseInFile))
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
            RequestAnswer result;

            using (_globalScope.Resolve<IWaitCursor>())
            {
                result = await _grpcC2DService.SendEventSourcingCommand(cmd);
            }

            var vm = result.ReturnCode != ReturnCode.Ok
                ? new MyMessageBoxViewModel(MessageType.Error, result.ReturnCode.GetLocalizedString())
                : new MyMessageBoxViewModel(MessageType.Information, Resources.SID_License_applied_successfully_);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            return result.ReturnCode != ReturnCode.Ok;
        }

        private async Task<bool>  IsCorrectPasswordEntered(LicenseInFile licenseInFile)
        {
            var vm = new SecurityAdminConfirmationViewModel();
            vm.Initialize(licenseInFile.Lk());
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            if (vm.IsOkPressed)
            {
                var password = (string)Cryptography.Decode(licenseInFile.SecurityAdminPassword!);
                if (vm.PasswordViewModel.Password != password)
                {
                    var strs = new List<string>() { Resources.SID_Wrong_password, "", Resources.SID_License_will_not_be_applied_ };
                    var mb = new MyMessageBoxViewModel(MessageType.Information, strs, 0);
                    await _windowManager.ShowDialogWithAssignedOwner(mb);
                    return false;
                }
            }
            else
            {
                var strs = new List<string>() { Resources.SID_Password_is_not_entered_, "", Resources.SID_License_will_not_be_applied_ };
                var mb = new MyMessageBoxViewModel(MessageType.Information, strs, 0);
                await _windowManager.ShowDialogWithAssignedOwner(mb);
                return false;
            }

            SecurityAdminPassword = vm.PasswordViewModel.Password;
            return true;
        }
    }
}