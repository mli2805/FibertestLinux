using System;
using System.Collections.Generic;
using System.Linq;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public static class GraphRendererCommonDetails
    {
        public static RenderingResult RenderRtus(this RenderingResult renderingResult, GraphReadModel graphReadModel, Guid zoneId)
        {
            foreach (var rtu in graphReadModel.ReadModel.Rtus.Where(r=>r.ZoneIds.Contains(zoneId)))
            {
                var nodeRtu = graphReadModel.ReadModel.Nodes.First(n => n.NodeId == rtu.NodeId);
                if (graphReadModel.MainMap.Limits.IsInPlus(nodeRtu.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(nodeRtu));
            }

            return renderingResult;
        }

        public static bool FindFiberNodes(Fiber fiber, Model readModel, RenderingResult renderingResult,
            List<NodeVm> nodesNear, out NodeVm? nodeVm1, out NodeVm? nodeVm2)
        {
            nodeVm1 = renderingResult.NodeVms.FirstOrDefault(n => n.Id == fiber.NodeId1);
            nodeVm2 = renderingResult.NodeVms.FirstOrDefault(n => n.Id == fiber.NodeId2);

            #region One node of the fiber is on screen while other is out
            if (nodeVm1 != null && nodeVm2 == null)
                nodeVm2 = FindNeighbor(fiber.NodeId2, readModel, nodesNear);
            if (nodeVm1 == null && nodeVm2 != null)
                nodeVm1 = FindNeighbor(fiber.NodeId1, readModel, nodesNear);
            #endregion

            return nodeVm1 != null && nodeVm2 != null;
        }

        private static NodeVm? FindNeighbor(Guid nodeId, Model readModel, List<NodeVm> nodesNear)
        {
            var neighbor = nodesNear.FirstOrDefault(n => n.Id == nodeId);
            if (neighbor == null)
            {
                var node = readModel.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
                if (node == null) return null;
                neighbor = ElementRenderer.Map(node);
                nodesNear.Add(neighbor);
            }

            return neighbor;
        }
    }
}