using System.Linq;
using System.Threading.Tasks;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class RtuVmActions
    {
        public async Task AskUpdateRtu(object parameter)
        {
            var marker = (MarkerControl)parameter;
            await Task.Delay(0);
            marker.Owner.GraphReadModel.GrmRtuRequests.UpdateRtu(new RequestUpdateRtu() { NodeId = marker.GMapMarker.Id });
        }

        public async Task AskRemoveRtu(object parameter)
        {
            var marker = (MarkerControl)parameter;

            await marker.Owner.GraphReadModel.GrmRtuRequests.RemoveRtu(new RequestRemoveRtu() { NodeId = marker.GMapMarker.Id });
        }

        // public void StartDefineTrace(object parameter)
        // {
        //     var marker = (MarkerControl)parameter;
        //
        //     marker.Owner.SetBanner(StringResources.Resources.SID_Trace_definition);
        //     marker.MainMap.IsInTraceDefiningMode = true;
        //     marker.MainMap.StartNode = marker.GMapMarker;
        // }

        public async Task StartDefineTraceStepByStep(object parameter)
        {
            var marker = (MarkerControl)parameter;
            await Task.Delay(0);
            marker.Owner.GraphReadModel.GrmRtuRequests.DefineTraceStepByStep(marker.GMapMarker.Id, marker.Title);
        }

        public async Task RevealTraces(object parameter)
        {
            var marker = (MarkerControl)parameter;

            var rtu = marker.Owner.GraphReadModel.ReadModel.Rtus.First(r => r.NodeId == marker.GMapMarker.Id);
            foreach (var trace in marker.Owner.GraphReadModel.ReadModel.Traces.Where(t => t.RtuId == rtu.Id))
            {
                if (!marker.Owner.GraphReadModel.ForcedTraces.Contains(trace))
                    marker.Owner.GraphReadModel.ForcedTraces.Add(trace);
            }

            await marker.Owner.GraphReadModel.RefreshVisiblePart();
        }
    }
}