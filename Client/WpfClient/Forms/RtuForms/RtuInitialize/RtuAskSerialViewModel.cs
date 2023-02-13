using Caliburn.Micro;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class RtuAskSerialViewModel : Screen
    {
        public string Message1 { get; set; } = null!;
        //public string Message2 { get; set; }
        public string Message3 { get; set; } = null!;

        public string Serial { get; set; } = null!;

        public bool IsSavePressed;

        public void Initialize(bool isFirstInitialization, string address, string oldSerial)
        {
            IsSavePressed = false;

            Message1 = string.Format(Resources.SID_Unauthorized_access_to_RTU__0__, address);
            //Message2 = isFirstInitialization ? "First initialization" : "Probably RTU was changed";
            Message3 = Resources.SID_Please_enter_Platform_serial_number;

            Serial = oldSerial;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
        }

        public async void Continue()
        {
            IsSavePressed = true;
            await TryCloseAsync();
        }

        public async void Cancel()
        {
            IsSavePressed = false;
            await TryCloseAsync();
        }
    }
}
