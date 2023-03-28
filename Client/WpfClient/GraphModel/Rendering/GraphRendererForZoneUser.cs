using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public static class GraphRendererForZoneUser
    {
        /// <summary>
        /// for one responsibility zone user (not default zone)
        /// </summary>
        /// <param name="graphReadModel"></param>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        public static async Task<RenderingResult> RenderForZoneUser(this GraphReadModel graphReadModel, Guid zoneId)
        {
            await Task.Delay(1);
            if (graphReadModel.MainMap == null)
                return new RenderingResult();
            
            return new RenderingResult()
                .RenderRtus(graphReadModel, zoneId)
                .RenderTraces(graphReadModel, zoneId)
                .RenderAccidents(graphReadModel, zoneId);
        }

        private static RenderingResult RenderTraces(this RenderingResult renderingResult, GraphReadModel graphReadModel, Guid zoneId)
        {
            return graphReadModel.MainMap.Zoom < graphReadModel.CurrentGis.ThresholdZoom
                ? renderingResult.RenderForcedTraces(graphReadModel)
                : renderingResult
                    .RenderAllTraceNodes(graphReadModel, zoneId)
                    .RenderAllTraceFibers(graphReadModel, zoneId);
        }

        private static RenderingResult RenderForcedTraces(this RenderingResult renderingResult,
            GraphReadModel graphReadModel)
        {
            var forcedTraces = graphReadModel.ForcedTraces.ToList();
            return renderingResult
                .RenderForcedTracesNodes(graphReadModel, forcedTraces)
                .RenderForcedTracesFibers(graphReadModel, forcedTraces);
        }

        private static RenderingResult RenderForcedTracesNodes(this RenderingResult renderingResult,
            GraphReadModel graphReadModel, List<Trace> forcedTraces)
        {
            var existingNodes = new HashSet<Guid>();
            foreach (var trace in forcedTraces)
            {
                foreach (var nodeId in trace.NodeIds)
                {
                    if (!existingNodes.Contains(nodeId))
                    {
                        existingNodes.Add(nodeId);
                        var node = graphReadModel.ReadModel.Nodes.First(n => n.NodeId == nodeId);
                        if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                            renderingResult.NodeVms.Add(ElementRenderer.Map(node));
                    }
                }
            }

            return renderingResult;
        }

        private static RenderingResult RenderForcedTracesFibers(this RenderingResult renderingResult,
            GraphReadModel graphReadModel, List<Trace> forcedTraces)
        {
            var nodesNear = new List<NodeVm>();
            var checkedFibers = new HashSet<Guid>();
            foreach (var trace in forcedTraces)
            {
                foreach (var fiberId in trace.FiberIds)
                {
                    if (!checkedFibers.Contains(fiberId))
                    {
                        checkedFibers.Add(fiberId);
                        var fiber = graphReadModel.ReadModel.Fibers.First(f => f.FiberId == fiberId);
                        if (GraphRendererCommonDetails.FindFiberNodes(
                                fiber, graphReadModel.ReadModel, renderingResult, nodesNear, out NodeVm? nodeVm1, out NodeVm? nodeVm2))
                            renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1!, nodeVm2!));
                    }
                }
            }
            renderingResult.NodeVms.AddRange(nodesNear);
            return renderingResult;
        }

        private static RenderingResult RenderAllTraceNodes(this RenderingResult renderingResult,
            GraphReadModel graphReadModel, Guid zoneId)
        {
            var allTracesNodes = new HashSet<Guid>();
            foreach (var trace in graphReadModel.ReadModel.Traces.Where(t=>t.ZoneIds.Contains(zoneId)))
                allTracesNodes.UnionWith(trace.NodeIds);

            foreach (var nodeId in allTracesNodes)
            {
                var node = graphReadModel.ReadModel.Nodes.First(n => n.NodeId == nodeId);
                if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));

            }
            return renderingResult;
        }

        private static RenderingResult RenderAllTraceFibers(this RenderingResult renderingResult,
            GraphReadModel graphReadModel, Guid zoneId)
        {
            var allTracesFibers = new HashSet<Guid>();
            foreach (var trace in graphReadModel.ReadModel.Traces.Where(t => t.ZoneIds.Contains(zoneId)))
                allTracesFibers.UnionWith(trace.FiberIds);

            var nodesNear = new List<NodeVm>();
            foreach (var fiberId in allTracesFibers)
            {
                var fiber = graphReadModel.ReadModel.Fibers.First(f => f.FiberId == fiberId);
                if (GraphRendererCommonDetails.FindFiberNodes(fiber, graphReadModel.ReadModel, renderingResult, nodesNear,
                        out NodeVm? nodeVm1, out NodeVm? nodeVm2))
                    renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1!, nodeVm2!));
            }

            renderingResult.NodeVms.AddRange(nodesNear);
            return renderingResult;
        }

        private static RenderingResult RenderAccidents(this RenderingResult renderingResult, GraphReadModel graphReadModel, Guid zoneId)
        {
            foreach (var node in graphReadModel.ReadModel.Nodes.Where(n=>n.AccidentOnTraceId != Guid.Empty))
            {
                var trace = graphReadModel.ReadModel.Traces.First(t => t.TraceId == node.AccidentOnTraceId);
                if (trace.ZoneIds.Contains(zoneId))
                {
                    if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                        renderingResult.NodeVms.Add(ElementRenderer.Map(node));
                }
            }
            return renderingResult;
        }
    }
}