using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using GMap.NET;

namespace Fibertest.WpfClient
{
    public class GpsInputViewModel : PropertyChangedBase
    {
        private readonly CurrentGis _currentGis;
        private readonly GraphReadModel _graphReadModel;
        private readonly TabulatorViewModel _tabulatorViewModel;

        public OneCoorViewModel OneCoorViewModelLatitude { get; set; }
        public OneCoorViewModel OneCoorViewModelLongitude { get; set; }

        public PointLatLng Coors { get; set; }

        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
        (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
         select new GpsInputModeComboItem(mode)).ToList();

        private readonly GpsInputMode _modeInIniFile;
        private GpsInputModeComboItem _selectedGpsInputModeComboItem;
        public GpsInputModeComboItem SelectedGpsInputModeComboItem
        {
            get => _selectedGpsInputModeComboItem;
            set
            {
                if (Equals(value, _selectedGpsInputModeComboItem)) return;
                _selectedGpsInputModeComboItem = value;
                OneCoorViewModelLatitude.CurrentGpsInputMode = value.Mode;
                OneCoorViewModelLongitude.CurrentGpsInputMode = value.Mode;
                _currentGis.GpsInputMode = _selectedGpsInputModeComboItem.Mode;
            }
        }


        public bool IsEditEnabled { get; set; }
        private Guid _originalNodeId;
        public GpsInputViewModel(CurrentGis currentGis, GraphReadModel graphReadModel, TabulatorViewModel tabulatorViewModel)
        {
            _currentGis = currentGis;
            _graphReadModel = graphReadModel;
            _tabulatorViewModel = tabulatorViewModel;
            _modeInIniFile = currentGis.GpsInputMode;
            _selectedGpsInputModeComboItem = _modeInIniFile == GpsInputMode.Degrees
                ? new GpsInputModeComboItem(GpsInputMode.DegreesAndMinutes)
                : new GpsInputModeComboItem(GpsInputMode.Degrees);
        }

        public void Initialize(Node originalNode, bool isEditEnabled)
        {
            _originalNodeId = originalNode.NodeId;
            Coors = originalNode.Position;

            OneCoorViewModelLatitude = new OneCoorViewModel(SelectedGpsInputModeComboItem.Mode, Coors.Lat);
            OneCoorViewModelLongitude = new OneCoorViewModel(SelectedGpsInputModeComboItem.Mode, Coors.Lng);
            SelectedGpsInputModeComboItem = GpsInputModes.FirstOrDefault(i => i.Mode == _modeInIniFile);

            IsEditEnabled = isEditEnabled;
        }

        public string TryGetPoint(out PointLatLng point)
        {
            point = new PointLatLng();
            if (!OneCoorViewModelLatitude.TryGetValue(out double lat)) return OneCoorViewModelLatitude.Error;
            if (!OneCoorViewModelLongitude.TryGetValue(out double lng)) return OneCoorViewModelLongitude.Error;
            point = new PointLatLng(lat, lng);
            return null;
        }

        public void PreView()
        {
            var nodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _originalNodeId);
            if (nodeVm == null) return;

            nodeVm.Position = new PointLatLng(OneCoorViewModelLatitude.StringsToValue(),
                OneCoorViewModelLongitude.StringsToValue());

            _graphReadModel.PlacePointIntoScreenCenter(nodeVm.Position);
            if (_tabulatorViewModel.SelectedTabIndex != 3)
                _tabulatorViewModel.SelectedTabIndex = 3;
        }

        public void DropChanges()
        {
            OneCoorViewModelLatitude.ReassignValue(Coors.Lat);
            OneCoorViewModelLongitude.ReassignValue(Coors.Lng);
        }

        public void ButtonDropChanges()
        {
            DropChanges();
            PreView();
        }
    }
}
