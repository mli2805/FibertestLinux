using System.ComponentModel;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class OneCoorViewModel : PropertyChangedBase, IDataErrorInfo
    {
        private string _degrees = null!;
        public string Degrees
        {
            get => _degrees;
            set
            {
                if (value == _degrees) return;
                _degrees = value;
                NotifyOfPropertyChange();
            }
        }

        private string _minutes = null!;
        public string Minutes
        {
            get => _minutes;
            set
            {
                if (value == _minutes) return;
                _minutes = value;
                NotifyOfPropertyChange();
            }
        }

      
        private string _seconds = null!;
        public string Seconds
        {
            get => _seconds;
            set
            {
                if (value == _seconds) return;
                _seconds = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _snapshot;

        private Visibility _degreesModeVisibility;
        public Visibility DegreesModeVisibility
        {
            get => _degreesModeVisibility;
            set
            {
                if (value == _degreesModeVisibility) return;
                _degreesModeVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _degreesAndMinutesModeVisibility;
        public Visibility DegreesAndMinutesModeVisibility
        {
            get => _degreesAndMinutesModeVisibility;
            set
            {
                if (value == _degreesAndMinutesModeVisibility) return;
                _degreesAndMinutesModeVisibility = value;
                NotifyOfPropertyChange();
            }
        }

       
        private Visibility _degreesMinutesAndSecondsModeVisibility;
        public Visibility DegreesMinutesAndSecondsModeVisibility
        {
            get => _degreesMinutesAndSecondsModeVisibility;
            set
            {
                if (value == _degreesMinutesAndSecondsModeVisibility) return;
                _degreesMinutesAndSecondsModeVisibility = value;
                NotifyOfPropertyChange();
            }
        }


        private GpsInputMode _currentGpsInputMode;
        public GpsInputMode CurrentGpsInputMode
        {
            get => _currentGpsInputMode;
            set
            {
                var flag = IsChanged();
                var temp = StringsToValue();
                _currentGpsInputMode = value;
                ChangeVisibilities();
                if (flag)
                    _value = temp;
                ValueToStrings();
            }
        }

        private void ChangeVisibilities()
        {
            switch (_currentGpsInputMode)
            {
                case GpsInputMode.Degrees:
                    DegreesModeVisibility = Visibility.Visible;
                    DegreesAndMinutesModeVisibility = Visibility.Collapsed;
                    DegreesMinutesAndSecondsModeVisibility = Visibility.Collapsed;
                    break;
                case GpsInputMode.DegreesAndMinutes:
                    DegreesModeVisibility = Visibility.Collapsed;
                    DegreesAndMinutesModeVisibility = Visibility.Visible;
                    DegreesMinutesAndSecondsModeVisibility = Visibility.Collapsed;
                    break;
                case GpsInputMode.DegreesMinutesAndSeconds:
                    DegreesModeVisibility = Visibility.Collapsed;
                    DegreesAndMinutesModeVisibility = Visibility.Collapsed;
                    DegreesMinutesAndSecondsModeVisibility = Visibility.Visible;
                    break;
            }
        }

        private double _value;
        public void ReassignValue(double newValue)
        {
            _value = newValue;
            ValueToStrings();
        }
        private void ValueToStrings()
        {
            if (CurrentGpsInputMode == GpsInputMode.Degrees)
            {
                Degrees = $@"{_value:#0.000000}";
            }
            else if (CurrentGpsInputMode == GpsInputMode.DegreesAndMinutes)
            {
                int d = (int)_value;
                Degrees = $@"{d:#0}";
                double m = (_value - d) * 60;
                Minutes = $@"{m:#0.0000}";
            }
            else if (CurrentGpsInputMode == GpsInputMode.DegreesMinutesAndSeconds)
            {
                int d = (int)_value;
                Degrees = $@"{d:#0}";
                double m = (_value - d) * 60;
                int mi = (int)m;
                Minutes = $@"{mi:#0}";
                double s = (m - mi) * 60;
                Seconds = $@"{s:#0.00}";
            }
            _snapshot = TakeSnapShot();
        }

        private bool IsChanged()
        {
            return TakeSnapShot() != _snapshot;
        }

        private string TakeSnapShot()
        {
            if (CurrentGpsInputMode == GpsInputMode.Degrees) return Degrees;
            if (CurrentGpsInputMode == GpsInputMode.DegreesAndMinutes) return Degrees + Minutes;
            return Degrees + Minutes + Seconds;
        }

        // this function is called if only something was changed by user
        public double StringsToValue()
        {
            if (string.IsNullOrEmpty(Degrees)) Degrees = @"0";
            if (string.IsNullOrEmpty(Minutes)) Minutes = @"0";
            if (string.IsNullOrEmpty(Seconds)) Seconds = @"0";

            if (CurrentGpsInputMode == GpsInputMode.Degrees)
                return double.Parse(Degrees);
            if (CurrentGpsInputMode == GpsInputMode.DegreesAndMinutes)
                return double.Parse(Degrees) + double.Parse(Minutes) / 60;
            return double.Parse(Degrees) + double.Parse(Minutes) / 60 + double.Parse(Seconds) / 3600;
        }

        public bool TryGetValue(out double value)
        {
            value = 0;

            if (string.IsNullOrEmpty(Degrees)) return false;
            if (CurrentGpsInputMode == GpsInputMode.Degrees)
                return double.TryParse(Degrees, out value);

            if (string.IsNullOrEmpty(Minutes)) return false;
            if (CurrentGpsInputMode == GpsInputMode.DegreesAndMinutes)
            {
                if (!double.TryParse(Degrees, out double degrees) || !double.TryParse(Minutes, out double minutes))
                    return false;
                value = degrees + minutes / 60;
                return true;
            }

            if (string.IsNullOrEmpty(Seconds)) return false;
            if (!double.TryParse(Degrees, out double degrees3) 
                || !double.TryParse(Minutes, out double minutes3)
                || !double.TryParse(Seconds, out double seconds3))
                return false;
            value = degrees3 + minutes3 / 60 + seconds3 / 3600;
            return true;
        }

        public OneCoorViewModel() { }

        public OneCoorViewModel(GpsInputMode currentGpsInputMode, double value)
        {
            _currentGpsInputMode = currentGpsInputMode;
            ChangeVisibilities();
            _value = value;
            ValueToStrings();
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Degrees":
                        if (string.IsNullOrEmpty(Degrees))
                            errorMessage = Resources.SID_Degrees_is_required;
                        if (!double.TryParse(Degrees, out double degrees))
                            errorMessage = Resources.SID_Invalid_input;
                        if (degrees < 0 || degrees >= 360) 
                            errorMessage = Resources.SID_Invalid_input;
                        Error = errorMessage;
                        break;
                    case "Minutes":
                        if (!double.TryParse(Minutes, out double minutes))
                            errorMessage = Resources.SID_Invalid_input;
                        if (minutes < 0 || minutes >= 60) 
                            errorMessage = Resources.SID_Invalid_input;
                        Error = errorMessage;
                        break;
                    case "Seconds":
                        if (!double.TryParse(Seconds, out double seconds))
                            errorMessage = Resources.SID_Invalid_input;
                        if (seconds < 0 || seconds >= 60) 
                            errorMessage = Resources.SID_Invalid_input;
                        Error = errorMessage;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; } = "";
    }
}
