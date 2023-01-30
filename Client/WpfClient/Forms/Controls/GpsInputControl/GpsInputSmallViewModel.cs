using Caliburn.Micro;
using GMap.NET;

namespace Fibertest.WpfClient
{
    public class GpsInputSmallViewModel : PropertyChangedBase
    {
        private readonly CurrentGis _currentGis;

        private OneCoorViewModel _oneCoorViewModelLatitude;
        public OneCoorViewModel OneCoorViewModelLatitude
        {
            get => _oneCoorViewModelLatitude;
            set
            {
                if (Equals(value, _oneCoorViewModelLatitude)) return;
                _oneCoorViewModelLatitude = value;
                NotifyOfPropertyChange();
            }
        }

        private OneCoorViewModel _oneCoorViewModelLongitude;
        public OneCoorViewModel OneCoorViewModelLongitude
        {
            get => _oneCoorViewModelLongitude;
            set
            {
                if (Equals(value, _oneCoorViewModelLongitude)) return;
                _oneCoorViewModelLongitude = value;
                NotifyOfPropertyChange();
            }
        }

        public PointLatLng Coors { get; set; }

        public void DropChanges()
        {
            OneCoorViewModelLatitude.ReassignValue(Coors.Lat);
            OneCoorViewModelLongitude.ReassignValue(Coors.Lng);
        }

        public GpsInputSmallViewModel(CurrentGis currentGis)
        {
            _currentGis = currentGis;
            currentGis.PropertyChanged += CurrentGpsInputMode_PropertyChanged;
        }

        private void CurrentGpsInputMode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OneCoorViewModelLatitude.CurrentGpsInputMode = ((CurrentGis) sender).GpsInputMode;
            OneCoorViewModelLongitude.CurrentGpsInputMode = ((CurrentGis) sender).GpsInputMode;
        }

        public void Initialize(PointLatLng coors)
        {
            Coors = coors;

            OneCoorViewModelLatitude = new OneCoorViewModel(_currentGis.GpsInputMode, Coors.Lat);
            OneCoorViewModelLongitude = new OneCoorViewModel(_currentGis.GpsInputMode, Coors.Lng);
        }

        public string TryGetPoint(out PointLatLng point)
        {
            point = new PointLatLng();
            if (!OneCoorViewModelLatitude.TryGetValue(out double lat)) return OneCoorViewModelLatitude.Error;
            if (!OneCoorViewModelLongitude.TryGetValue(out double lng)) return OneCoorViewModelLongitude.Error;
            point = new PointLatLng(lat, lng);
            return null;
        }

    }
}
