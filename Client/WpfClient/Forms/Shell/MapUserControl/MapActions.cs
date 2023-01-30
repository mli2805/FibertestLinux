using System.Threading.Tasks;
using System.Windows;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class MapActions
    {
        private readonly GraphReadModel _graphReadModel;

        public MapActions(GraphReadModel graphReadModel)
        {
            _graphReadModel = graphReadModel;
        }

        public async Task AddNodeOnClick(object param)
        {
            if (!(param is EquipmentType equipmentType)) return;

            var position = _graphReadModel.MainMap.FromLocalToLatLng(_graphReadModel.MainMap.ContextMenuPoint);

            if (equipmentType == EquipmentType.Rtu)
                _graphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(
                    new RequestAddRtuAtGpsLocation() { Latitude = position.Lat, Longitude = position.Lng });

            else
            {
                var expectedVisibilityLevel = equipmentType == EquipmentType.EmptyNode
                    ? GraphVisibilityLevel.EmptyNodes
                    : GraphVisibilityLevel.Equipments;
                if (_graphReadModel.SelectedGraphVisibilityItem.Level < expectedVisibilityLevel)
                    _graphReadModel.SetGraphVisibility(expectedVisibilityLevel);

                await _graphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(
                    new RequestAddEquipmentAtGpsLocation()
                    {
                        Type = equipmentType,
                        Latitude = position.Lat,
                        Longitude = position.Lng
                    });
            }
        }

        public async Task CopyCoordinatesToClipboard(object parameter)
        {
            await Task.Delay(0);
            var position = _graphReadModel.MainMap.FromLocalToLatLng(_graphReadModel.MainMap.ContextMenuPoint);
            Clipboard.SetText(position.ToString());
        }

        public async Task ToggleToDistanceMeasurementMode(object parameter)
        {
            await Task.Delay(0);
            if (!_graphReadModel.MainMap.IsInDistanceMeasurementMode)
            {
                _graphReadModel.CommonStatusBarViewModel.StatusBarMessage2 = StringResources.Resources.SID_Distance_measurement_mode;
                _graphReadModel.MainMap.IsInDistanceMeasurementMode = true;
                _graphReadModel.MainMap.StartNode = null;
            }
            else
            {
                _graphReadModel.MainMap.LeaveDistanceMeasurementMode();
                _graphReadModel.MainMap.IsInDistanceMeasurementMode = false;
                _graphReadModel.CommonStatusBarViewModel.StatusBarMessage2 = "";
            }
        }

        public bool CanAddNode(object parameter)
        {
            return _graphReadModel.CurrentUser.Role <= Role.Root;
        }

        public bool Can(object parameter)
        {
            return true;
        }


    }
}