using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class TceInfoViewModel : PropertyChangedBase
    {
        private string _title;

        public string Title 
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public Ip4InputViewModel Ip4InputViewModel { get; set; }

        public bool ProcessSnmpTraps { get; set; }
        public string Comment { get; set; }

        public TceS Tce { get; set; }

        private bool _isEnabled;
        public bool IsEnabled // edit control 
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public void Initialize(TceS tce, bool isEnabled)
        {
            Tce = tce;

            Title = tce.Title;
            Ip4InputViewModel = new Ip4InputViewModel(tce.Ip);
            ProcessSnmpTraps = tce.ProcessSnmpTraps;
            Comment = tce.Comment;

            IsEnabled = isEnabled;
        }
    }
}
