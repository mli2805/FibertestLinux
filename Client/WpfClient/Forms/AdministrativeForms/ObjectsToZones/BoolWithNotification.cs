using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class BoolWithNotification : PropertyChangedBase
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                NotifyOfPropertyChange();
            }
        }
    }
}