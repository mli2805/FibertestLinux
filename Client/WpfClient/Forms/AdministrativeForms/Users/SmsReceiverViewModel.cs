using System;
using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class SmsReceiverViewModel : PropertyChangedBase
    {
        private string _phoneNumber;
        private bool _isFiberBreakOn;
        private bool _isCriticalOn;
        private bool _isMajorOn;
        private bool _isMinorOn;
        private bool _isActivated;
        private bool _isOkOn;
        private bool _isNetworkEventsOn;
        private bool _isBopEventsOn;

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (value == _phoneNumber) return;
                _phoneNumber = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsFiberBreakOn
        {
            get => _isFiberBreakOn;
            set
            {
                if (value == _isFiberBreakOn) return;
                _isFiberBreakOn = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsCriticalOn
        {
            get => _isCriticalOn;
            set
            {
                if (value == _isCriticalOn) return;
                _isCriticalOn = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsMajorOn
        {
            get => _isMajorOn;
            set
            {
                if (value == _isMajorOn) return;
                _isMajorOn = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsMinorOn
        {
            get => _isMinorOn;
            set
            {
                if (value == _isMinorOn) return;
                _isMinorOn = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsOkOn
        {
            get => _isOkOn;
            set
            {
                if (value == _isOkOn) return;
                _isOkOn = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsNetworkEventsOn
        {
            get { return _isNetworkEventsOn; }
            set
            {
                if (value == _isNetworkEventsOn) return;
                _isNetworkEventsOn = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsBopEventsOn
        {
            get { return _isBopEventsOn; }
            set
            {
                if (value == _isBopEventsOn) return;
                _isBopEventsOn = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsActivated
        {
            get => _isActivated;
            set
            {
                if (value == _isActivated) return;
                _isActivated = value;
                NotifyOfPropertyChange();
            }
        }

        public event EventHandler OnTestButtonPressed;
        public void SendTestSms()
        {
            OnTestButtonPressed?.Invoke(this, null);
        }

        public SmsReceiver Get()
        {
            return new SmsReceiver()
            {
                PhoneNumber = PhoneNumber,
                IsActivated = IsActivated,
                IsFiberBreakOn = IsFiberBreakOn,
                IsCriticalOn = IsCriticalOn,
                IsMajorOn = IsMajorOn,
                IsMinorOn = IsMinorOn,
                IsOkOn = IsOkOn,
                IsNetworkEventsOn = IsNetworkEventsOn,
                IsBopEventsOn = IsBopEventsOn,
            };
        }
    }
}
