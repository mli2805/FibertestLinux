using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class SmsSettingsViewModel : Screen
    {
        private readonly DataCenterConfig _currentDatacenterParameters;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public string GsmModemComPort { get; set; }
        public bool IsEditEnabled { get; set; }

        public SmsSettingsViewModel(DataCenterConfig currentDatacenterParameters, CurrentUser currentUser,
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            GsmModemComPort = _currentDatacenterParameters.Broadcast.GsmModemComPort;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_SMS_settings;
        }

        public async void Save()
        {
            bool res;
            using (new WaitCursor())
            {
                _currentDatacenterParameters.Broadcast.GsmModemComPort = GsmModemComPort;

                res = await _c2DWcfManager.SaveGsmComPort(GsmModemComPort);
            }

            if (res)
                await TryCloseAsync();
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Failed_to_save_gsm_modem_com_port_);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }


        public async void Cancel()
        {
            await TryCloseAsync();
        }
    }
}
