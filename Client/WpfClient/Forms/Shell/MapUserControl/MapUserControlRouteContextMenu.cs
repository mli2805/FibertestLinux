using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Fibertest.Dto;
using Fibertest.Graph;
using GMap.NET.WindowsPresentation;

namespace Fibertest.WpfClient
{
    /// <summary>
    /// on route creation subscribes on event that means 
    /// user clicked right mouse button on this route 
    /// 
    /// here we build ContextMenu for this particular route in runtime
    /// and here are reactions on MenuItems
    /// </summary>
    public partial class MapUserControl
    {
        private bool CanUpdateFiber(object? parameter)
        {
            return parameter != null;
        }

        private async Task AskUpdateFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            await GraphReadModel.GrmFiberRequests.UpdateFiber(new RequestUpdateFiber() { Id = route.Id });
        }

        private bool CanAddNodeIntoFiber(object? parameter)
        {
            if (GraphReadModel.CurrentUser.Role > Role.Root || parameter == null)
                return false;
            var route = (GMapRoute)parameter;
            return !GraphReadModel.GrmNodeRequests.IsFiberContainedInAnyTraceWithBase(route.Id);
        }
        private async Task AskAddNodeIntoFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            var position = MainMap.FromLocalToLatLng(MainMap.ContextMenuPoint);
            await GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() { FiberId = route.Id, InjectionType = EquipmentType.EmptyNode, Position = position });
        }

        private bool CanAddAdjustmentNodeIntoFiber(object? parameter)
        {
            return GraphReadModel.CurrentUser.Role <= Role.Root && parameter != null;
        }
        private async Task AskAddAdjustmentNodeIntoFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            var position = MainMap.FromLocalToLatLng(MainMap.ContextMenuPoint);
            await GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() { FiberId = route.Id, InjectionType = EquipmentType.AdjustmentPoint, Position = position });
        }

        private bool CanRemoveFiber(object? parameter)
        {
            if (GraphReadModel.CurrentUser.Role > Role.Root || parameter == null)
                return false;
            var fiberVm = GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == ((GMapRoute)parameter).Id);
            return fiberVm?.State == FiberState.NotInTrace;
        }

        private async Task AskRemoveFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            await GraphReadModel.GrmFiberRequests.RemoveFiber(new RemoveFiber() { FiberId = route.Id });
        }

        private void Route_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"AskContextMenu")
            {
                var route = (GMapRoute)sender!;
                SetupContextMenu(route);
            }
        }

        private void SetupContextMenu(GMapRoute route)
        {
            route.ContextMenu = new ContextMenu();
            route.ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Information,
                Command = new ContextMenuAsyncAction(AskUpdateFiber, CanUpdateFiber),
                CommandParameter = route
            });
            route.ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Add_node,
                Command = new ContextMenuAsyncAction(AskAddNodeIntoFiber, CanAddNodeIntoFiber),
                CommandParameter = route
            });
            route.ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Add_adjustment_point,
                Command = new ContextMenuAsyncAction(AskAddAdjustmentNodeIntoFiber, CanAddAdjustmentNodeIntoFiber),
                CommandParameter = route
            });
            route.ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Remove_section,
                Command = new ContextMenuAsyncAction(AskRemoveFiber, CanRemoveFiber),
                CommandParameter = route
            });
        }
    }
}
