using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class FreePorts : PropertyChangedBase
    {
        private bool _areVisible;
        public bool AreVisible
        {
            get => _areVisible;
            set
            {
                if (value == _areVisible) return;
                _areVisible = value;
                NotifyOfPropertyChange();
            }
        }
    }
}