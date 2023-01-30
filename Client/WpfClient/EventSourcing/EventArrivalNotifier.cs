using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class EventArrivalNotifier : PropertyChangedBase
    {
        private int _neverMind;

        public int NeverMind
        {
            get => _neverMind;
            set
            {
                if (value == _neverMind) return;
                _neverMind = value;
                NotifyOfPropertyChange();
            }
        }
    }
}