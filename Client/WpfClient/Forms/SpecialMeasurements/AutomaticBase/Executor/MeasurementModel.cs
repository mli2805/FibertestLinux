using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class MeasurementModel : PropertyChangedBase
    {
        public CurrentUser CurrentUser;
        public Rtu Rtu;
        public int MeasurementTimeout;

        public bool InterruptedPressed;

        public OtdrParametersTemplatesViewModel OtdrParametersTemplatesViewModel { get; set; }
        public AutoAnalysisParamsViewModel AutoAnalysisParamsViewModel { get; set; }
        public MeasurementProgressViewModel MeasurementProgressViewModel { get; set; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
                OtdrParametersTemplatesViewModel.IsEnabled = _isEnabled;
                AutoAnalysisParamsViewModel.IsEnabled = _isEnabled;
            }
        }

        private string _totalTraces;
        public string TotalTraces
        {
            get => _totalTraces;
            set
            {
                if (value == _totalTraces) return;
                _totalTraces = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<string> _traceResults = new ObservableCollection<string>();
        public ObservableCollection<string> TraceResults
        {
            get => _traceResults;
            set
            {
                if (Equals(value, _traceResults)) return;
                _traceResults = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _traceResultsVisibility = Visibility.Collapsed;
        public Visibility TraceResultsVisibility
        {
            get => _traceResultsVisibility;
            set
            {
                if (value == _traceResultsVisibility) return;
                _traceResultsVisibility = value;
                NotifyOfPropertyChange();
            }
        }
    }
}