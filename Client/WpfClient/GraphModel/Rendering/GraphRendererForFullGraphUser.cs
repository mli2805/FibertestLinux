using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fibertest.Graph;

// ReSharper disable ForCanBeConvertedToForeach

namespace Fibertest.WpfClient
{
    public static class GraphRendererForFullGraphUser
    {
        /// <summary>
        /// for any user (not only Root) from default zone
        /// </summary>
        /// <param name="graphReadModel"></param>
        /// <returns></returns>
        public static async Task<RenderingResult> RenderForFullGraphUser(this GraphReadModel graphReadModel)
        {
            await Task.Delay(1);
            if (graphReadModel.MainMap == null)
                return new RenderingResult();

            if (graphReadModel.MainMap.Zoom < graphReadModel.CurrentGis.ThresholdZoom)
            {
                var res = new RenderingResult().RenderRtus(graphReadModel, Guid.Empty);
                var forcedTraces = graphReadModel.ForcedTraces.ToList();
                return res.RenderForcedTracesAndNotInTracesNodes(graphReadModel, forcedTraces)
                    .RenderForcedTracesAndNotInTracesFibers(graphReadModel, forcedTraces);
            }

            return new RenderingResult()
                .RenderAllNodes(graphReadModel)
                .RenderAllFibers(graphReadModel);
        }

        private static RenderingResult RenderForcedTracesAndNotInTracesNodes(this RenderingResult renderingResult, 
            GraphReadModel graphReadModel, List<Trace> forcedTraces)
        {
            var allTracesNodes = new HashSet<Guid>();
            var forcedNodes = new HashSet<Guid>();
            foreach (var trace in graphReadModel.ReadModel.Traces)
            {
                allTracesNodes.UnionWith(trace.NodeIds);
                if (forcedTraces.Contains(trace))
                    forcedNodes.UnionWith(trace.NodeIds);
            }

            for (int i = 0; i < graphReadModel.ReadModel.Nodes.Count; i++)
            {
                if (forcedNodes.Contains(graphReadModel.ReadModel.Nodes[i].NodeId) 
                    || !allTracesNodes.Contains(graphReadModel.ReadModel.Nodes[i].NodeId))
                {
                    if (graphReadModel.MainMap.Limits.IsInPlus(graphReadModel.ReadModel.Nodes[i].Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                        renderingResult.NodeVms.Add(ElementRenderer.Map(graphReadModel.ReadModel.Nodes[i]));
                }
            }

            return renderingResult;
        }

        private static RenderingResult RenderForcedTracesAndNotInTracesFibers(this RenderingResult renderingResult,
            GraphReadModel graphReadModel, List<Trace> forcedTraces)
        {
            var allTracesFibers = new HashSet<Guid>();
            var forcedFibers = new HashSet<Guid>();
            foreach (var trace in graphReadModel.ReadModel.Traces)
            {
                allTracesFibers.UnionWith(trace.FiberIds);
                if (forcedTraces.Contains(trace))
                    forcedFibers.UnionWith(trace.FiberIds);
            }

            var nodesNear = new List<NodeVm>();
            for (int i = 0; i < graphReadModel.ReadModel.Fibers.Count; i++)
            {
                if (forcedFibers.Contains(graphReadModel.ReadModel.Fibers[i].FiberId)
                    || !allTracesFibers.Contains(graphReadModel.ReadModel.Fibers[i].FiberId))
                {
                    if (GraphRendererCommonDetails.FindFiberNodes(graphReadModel.ReadModel.Fibers[i], 
                            graphReadModel.ReadModel, renderingResult, nodesNear, out NodeVm? nodeVm1, out NodeVm? nodeVm2))
                        renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(graphReadModel.ReadModel.Fibers[i], nodeVm1!, nodeVm2!));
                }
            }

            renderingResult.NodeVms.AddRange(nodesNear);
            return renderingResult;
        }

        private static RenderingResult RenderAllNodes(this RenderingResult renderingResult, GraphReadModel graphReadModel)
        {
            for (int i = 0; i < graphReadModel.ReadModel.Nodes.Count; i++)
            {
                if (graphReadModel.MainMap.Limits.IsInPlus(graphReadModel.ReadModel.Nodes[i].Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(graphReadModel.ReadModel.Nodes[i]));
            }
            return renderingResult;
        }

        private static RenderingResult RenderAllFibers(this RenderingResult renderingResult, GraphReadModel graphReadModel)
        {
            var nodesNear = new List<NodeVm>();
            for (int i = 0; i < graphReadModel.ReadModel.Fibers.Count; i++)
            {
                if (GraphRendererCommonDetails.FindFiberNodes(
                        graphReadModel.ReadModel.Fibers[i], graphReadModel.ReadModel, renderingResult, nodesNear, out NodeVm? nodeVm1, out NodeVm? nodeVm2))
                    renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(graphReadModel.ReadModel.Fibers[i], nodeVm1!, nodeVm2!));
            }
            renderingResult.NodeVms.AddRange(nodesNear);
            return renderingResult;
        }

      
    }
}
