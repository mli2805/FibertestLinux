using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class SmtpSettingsViewModel : Screen
    {
        private readonly DataCenterConfig _currentDatacenterParameters;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly IWindowManager _windowManager;

        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string MailFrom { get; set; }
        public string MailFromPassword { get; set; }
        public int SmtpTimeoutMs { get; set; }

        public bool IsEditEnabled { get; set; }

        public SmtpSettingsViewModel(DataCenterConfig currentDatacenterParameters, CurrentUser currentUser,
            GrpcC2DService grpcC2DService, IWindowManager windowManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            _grpcC2DService = grpcC2DService;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _windowManager = windowManager;

            SmtpHost = _currentDatacenterParameters.Smtp.SmtpHost;
            SmtpPort = _currentDatacenterParameters.Smtp.SmtpPort;
            MailFrom = _currentDatacenterParameters.Smtp.MailFrom;
            MailFromPassword = _currentDatacenterParameters.Smtp.MailFromPassword;
            SmtpTimeoutMs = _currentDatacenterParameters.Smtp.SmtpTimeoutMs;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_E_mail_settings;
        }

        public async void Save()
        {
            RequestAnswer result;
            using (new WaitCursor())
            {
                var dto = new ChangeDcConfigDto() { NewConfig = _currentDatacenterParameters };
                dto.NewConfig.Smtp.SmtpHost = SmtpHost;
                dto.NewConfig.Smtp.SmtpPort = SmtpPort;
                dto.NewConfig.Smtp.MailFrom = MailFrom;
                dto.NewConfig.Smtp.MailFromPassword = MailFromPassword;
                dto.NewConfig.Smtp.SmtpTimeoutMs = SmtpTimeoutMs;

                result = await _grpcC2DService.SendAnyC2DRequest<ChangeDcConfigDto, RequestAnswer>(dto);
            }

            if (result.ReturnCode == ReturnCode.Ok)
            {
                _currentDatacenterParameters.Smtp.SmtpHost = SmtpHost;
                _currentDatacenterParameters.Smtp.SmtpPort = SmtpPort;
                _currentDatacenterParameters.Smtp.MailFrom = MailFrom;
                _currentDatacenterParameters.Smtp.MailFromPassword = MailFromPassword;
                _currentDatacenterParameters.Smtp.SmtpTimeoutMs = SmtpTimeoutMs;

                await TryCloseAsync();
            }
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Failed_to_save_smtp_settings_);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        public async void Cancel()
        {
            await TryCloseAsync();
        }
    }
}
