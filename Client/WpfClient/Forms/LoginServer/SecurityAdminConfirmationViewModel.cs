using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class SecurityAdminConfirmationViewModel : Screen
    {
        public string ReturnCodeText { get; set; } = "";
        public string Text3 { get; set; } = "";
        public string Text4 { get; set; } = "";
        public string Text5 { get; set; } = "";

        public PasswordViewModel PasswordViewModel { get; set; } = new PasswordViewModel();

        public bool IsOkPressed { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Password;
            PasswordViewModel.Password = "";
        }

        public void Initialize(ClientRegisteredDto resultDto)
        {
            ReturnCodeText = resultDto.ReturnCode.GetLocalizedString();
            if (resultDto.ReturnCode == ReturnCode.WrongMachineKey ||
                resultDto.ReturnCode == ReturnCode.EmptyMachineKey)
            {
                Text3 = Resources.SID_To_link_user_to_the_workstation_your_privileges;
                Text4 = Resources.SID_have_to_be_confirmed_by_security_administrator_password_;
            }
        }

        public void Initialize(string licenseKey)
        {
            Text3 = Resources.SID_To_apply_the_license;
            Text4 = licenseKey;
            Text5 = Resources.SID_security_administrator_password_has_to_be_input;
        }

        public async void OkButton()
        {
            IsOkPressed = true;
            await TryCloseAsync();
        }

        public async void CancelButton()
        {
            IsOkPressed = false;
            await TryCloseAsync();
        }

    }
}
