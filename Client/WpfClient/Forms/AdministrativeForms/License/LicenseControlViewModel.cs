using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class LicenseControlViewModel : PropertyChangedBase
    {
        private License _license;
        public License License
        {
            get => _license;
            set
            {
                if (Equals(value, _license)) return;
                _license = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsBasic));
            }
        }

        public bool IsBasic => !License.IsIncremental;
        public bool IsStandart => !License.IsMachineKeyRequired;
    }
}
