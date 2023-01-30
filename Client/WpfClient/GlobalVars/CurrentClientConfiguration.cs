using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class CurrentClientConfiguration : PropertyChangedBase
    {
        private bool _doNotSignalAboutSuspicion;
        public bool DoNotSignalAboutSuspicion
        {
            get => _doNotSignalAboutSuspicion;
            set
            {
                if (value == _doNotSignalAboutSuspicion) return;
                _doNotSignalAboutSuspicion = value;
                NotifyOfPropertyChange();
            }
        }
    }
}