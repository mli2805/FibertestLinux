using System.Windows.Controls;
using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class MapContextMenuProvider
    {
        private readonly MapActions _mapActions;

        public MapContextMenuProvider(MapActions mapActions)
        {
            _mapActions = mapActions;
        }

        public ContextMenu GetMapContextMenu()
        {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node,
                Command = new ContextMenuAsyncAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.EmptyNode,
            });

            contextMenu.Items.Add(new Separator());

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node_with_cable_reserve,
                Command = new ContextMenuAsyncAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.CableReserve,
            });

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node_with_sleeve,
                Command = new ContextMenuAsyncAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.Closure,
            });

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node_with_cross,
                Command = new ContextMenuAsyncAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.Cross,
            });

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node_with_terminal,
                Command = new ContextMenuAsyncAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.Terminal,
            });

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node_with_other_equipment,
                Command = new ContextMenuAsyncAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.Other,
            });

            contextMenu.Items.Add(new Separator());

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_RTU,
                Command = new ContextMenuAsyncAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.Rtu,
            });

            contextMenu.Items.Add(new Separator());

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Copy_coordinates_to_clipboard,
                Command = new ContextMenuAsyncAction(_mapActions.CopyCoordinatesToClipboard, _mapActions.Can),
                CommandParameter = null,
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Distace_measurement,
                Command = new ContextMenuAsyncAction(_mapActions.ToggleToDistanceMeasurementMode, _mapActions.Can),
                CommandParameter = null,
            });

            return contextMenu;
        }
    }
}