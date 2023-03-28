using Caliburn.Micro;
using GMap.NET;

namespace Fibertest.WpfClient
{
    public class TraceStateModelHeader : PropertyChangedBase
    {
        private string _traceTitle = "";
        private string _rtuTitle = "";

        public string TraceTitle
        {
            get => _traceTitle;
            set
            {
                if (value == _traceTitle) return;
                _traceTitle = value;
                NotifyOfPropertyChange();
            }
        }

        public string RtuTitle
        {
            get => _rtuTitle;
            set
            {
                if (value == _rtuTitle) return;
                _rtuTitle = value;
                NotifyOfPropertyChange();
            }
        }

        public string PortTitle { get; set; } = "";

        public PointLatLng? RtuPosition { get; set; }
        public string RtuSoftwareVersion { get; set; } = "";
        public string RtuCompilation => $@"{RtuTitle} (v. {RtuSoftwareVersion})";

        public string ServerTitle { get; set; } = string.Empty;

    }
}