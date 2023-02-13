using System;
using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class DbOptimizationModel : PropertyChangedBase
    {
        #region statistics information
        private int _measurementsNotEvents;
        public int MeasurementsNotEvents
        {
            get => _measurementsNotEvents;
            set
            {
                if (value == _measurementsNotEvents) return;
                _measurementsNotEvents = value;
                NotifyOfPropertyChange();
            }
        }

        private int _opticalEvents;
        public int OpticalEvents
        {
            get => _opticalEvents;
            set
            {
                if (value == _opticalEvents) return;
                _opticalEvents = value;
                NotifyOfPropertyChange();
            }
        }

        private int _networkEvents;
        public int NetworkEvents
        {
            get => _networkEvents;
            set
            {
                if (value == _networkEvents) return;
                _networkEvents = value;
                NotifyOfPropertyChange();
            }
        }

        public string DriveSize { get; set; } = null!;
        public string DataSize { get; set; } = null!;
        public string AvailableFreeSpace { get; set; } = null!;
        public string FreeSpaceThreshold { get; set; } = null!;
        #endregion

        public bool IsRemoveMode { get; set; } = true;
        public  bool IsSnapshotMode { get; set; }

        public bool IsMeasurements { get; set; }
        public bool IsOpticalEvents { get; set; }
        public bool IsNetworkEvents { get; set; }


        #region remove sor
        public DateTime UpToLimit{ get; set; } 

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (value.Equals(_selectedDate)) return;
                _selectedDate = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(UpToLimit));
            }
        }
        #endregion

        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
            }
        }
    }

}