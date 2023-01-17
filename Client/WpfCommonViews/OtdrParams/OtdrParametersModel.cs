using System.Collections.Generic;
using Caliburn.Micro;

namespace WpfCommonViews
{
    public class OtdrParametersModel : PropertyChangedBase
    {
        private string _selectedUnit = null!;
        private double _backscatteredCoefficient;
        private double _refractiveIndex;
        private List<string> _distances = null!;
        private string _selectedDistance = null!;
        private List<string> _resolutions = null!;
        private string _selectedResolution = null!;
        private List<string> _pulseDurations = null!;
        private string _selectedPulseDuration = null!;
        private List<string> _measurementTime = null!;
        private string _selectedMeasurementTime = null!;

        public List<string> Units { get; set; } = null!;

        public string SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (value == _selectedUnit) return;
                _selectedUnit = value;
                NotifyOfPropertyChange();
            }
        } 

        public double BackscatteredCoefficient
        {
            get => _backscatteredCoefficient;
            set
            {
                if (value.Equals(_backscatteredCoefficient)) return;
                _backscatteredCoefficient = value;
                NotifyOfPropertyChange();
            }
        }

        public double RefractiveIndex
        {
            get => _refractiveIndex;
            set
            {
                if (value.Equals(_refractiveIndex)) return;
                _refractiveIndex = value;
                NotifyOfPropertyChange();
            }
        }

        public List<string> Distances
        {
            get => _distances;
            set
            {
                if (Equals(value, _distances)) return;
                _distances = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedDistance
        {
            get => _selectedDistance;
            set
            {
                if (value == _selectedDistance) return;
                _selectedDistance = value;
                NotifyOfPropertyChange();
            }
        }

        public List<string> Resolutions
        {
            get => _resolutions;
            set
            {
                if (Equals(value, _resolutions)) return;
                _resolutions = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedResolution
        {
            get => _selectedResolution;
            set
            {
                if (value == _selectedResolution) return;
                _selectedResolution = value;
                NotifyOfPropertyChange();
            }
        }

        public List<string> PulseDurations
        {
            get => _pulseDurations;
            set
            {
                if (Equals(value, _pulseDurations)) return;
                _pulseDurations = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedPulseDuration
        {
            get => _selectedPulseDuration;
            set
            {
                if (value == _selectedPulseDuration) return;
                _selectedPulseDuration = value;
                NotifyOfPropertyChange();
            }
        }

        public List<string> MeasurementTime
        {
            get => _measurementTime;
            set
            {
                if (Equals(value, _measurementTime)) return;
                _measurementTime = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedMeasurementTime
        {
            get => _selectedMeasurementTime;
            set
            {
                if (value == _selectedMeasurementTime) return;
                _selectedMeasurementTime = value;
                NotifyOfPropertyChange();
            }
        }
    }
}