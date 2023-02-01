using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class SmsSettingsViewModel : Screen
    {
        private readonly DataCenterConfig _currentDatacenterParameters;
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly IWindowManager _windowManager;

        public string GsmModemComPort { get; set; }
        public bool IsEditEnabled { get; set; }

        public SmsSettingsViewModel(DataCenterConfig currentDatacenterParameters, CurrentUser currentUser,
            GrpcC2DRequests grpcC2DRequests, IWindowManager windowManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            _grpcC2DRequests = grpcC2DRequests;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _windowManager = windowManager;

            GsmModemComPort = _currentDatacenterParameters.Broadcast.GsmModemComPort;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_SMS_settings;
        }

        public async void Save()
        {
            RequestAnswer result;
            using (new WaitCursor())
            {
                var dto = new ChangeDcConfigDto() { NewConfig = _currentDatacenterParameters };
                dto.NewConfig.Broadcast.GsmModemComPort = GsmModemComPort;

                result = await _grpcC2DRequests.SendAnyC2DRequest<ChangeDcConfigDto, RequestAnswer>(dto);
            }

            if (result.ReturnCode == ReturnCode.Ok)
            {
                _currentDatacenterParameters.Broadcast.GsmModemComPort = GsmModemComPort;
                await TryCloseAsync();
            }
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
