using Caliburn.Micro;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class MeasurementProgressViewModel : PropertyChangedBase
    {
        private bool _isProgressBarRunning;
        public bool IsProgressBarRunning
        {
            get => _isProgressBarRunning;
            set
            {
                if (value == _isProgressBarRunning) return;
                _isProgressBarRunning = value;
                NotifyOfPropertyChange();
            }
        }


        private string _message1 = "";
        public string Message1
        {
            get => _message1;
            set
            {
                if (value == _message1) return;
                _message1 = value;
                NotifyOfPropertyChange();
            }
        }

        private string _message = "";

        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                NotifyOfPropertyChange();
            }
        }

        public void DisplayStartMeasurement(string traceTitle)
        {
            Message1 = traceTitle;
            IsProgressBarRunning = true;
            Message = Resources.SID_Sending_command__Wait_please___;
        }

        public void DisplayStop()
        {
            Message = "";
            Message1 = "";
            IsProgressBarRunning = false;
        }

        public void DisplayFinishInProgress()
        {
            Message1 = "";
            Message = Resources.SID_The_process_is_finishing__Please_wait___;
        }
    }
}
