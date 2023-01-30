using Caliburn.Micro;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class PasswordViewModel : PropertyChangedBase
    {
        private string _password;
        public string Label { get; set; } = Resources.SID_Password;

        public string Password
        {
            get => _password;
            set
            {
                if (value == _password) return;
                _password = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
