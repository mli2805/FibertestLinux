using System.Threading.Tasks;

namespace Fibertest.WpfClient
{
    public class CommonVmActions
    {
        public async Task StartAddFiber(object parameter)
        {
            await Task.Delay(0);
            var marker = (MarkerControl)parameter;
            marker.MainMap.IsFiberWithNodes = false;
            marker.MainMap.IsInFiberCreationMode = true;
            marker.MainMap.StartNode = marker.GMapMarker;
        }

        public async Task StartAddFiberWithNodes(object parameter)
        {
            await Task.Delay(0);
            var marker = (MarkerControl)parameter;
            marker.MainMap.IsFiberWithNodes = true;
            marker.MainMap.IsInFiberCreationMode = true;
            marker.MainMap.StartNode = marker.GMapMarker;
        }
    }
}