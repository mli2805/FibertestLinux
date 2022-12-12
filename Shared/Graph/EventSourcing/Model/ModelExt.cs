using Fibertest.Dto;

namespace Fibertest.Graph
{
    public static class ModelExt
    {
        public static Guid GetFiberIdBetweenNodes(this Model model, Guid node1, Guid node2)
        {
            return model.Fibers.First(
                f => f.NodeId1 == node1 && f.NodeId2 == node2 ||
                     f.NodeId1 == node2 && f.NodeId2 == node1).FiberId;
        }
      
        public static IEnumerable<Fiber> GetNodeFibers(this Model model, Node node)
        {
            foreach (var fiber in model.Fibers)
                if (fiber.NodeId1 == node.NodeId || fiber.NodeId2 == node.NodeId)
                    yield return fiber;
        }

        public static IEnumerable<Guid> GetNodeNeighbours(this Model model, Guid nodeId)
        {
            for (int i = 0; i < model.Fibers.Count; i++)
            {
                if (model.Fibers[i].NodeId1 == nodeId)
                    yield return model.Fibers[i].NodeId2;
                if (model.Fibers[i].NodeId2 == nodeId)
                    yield return model.Fibers[i].NodeId1;
            }
        }

        public static Fiber GetAnotherFiberOfAdjustmentPoint(this Model model, Node adjustmentPoint, Guid fiberId)
        {
            return model.GetNodeFibers(adjustmentPoint).First(f => f.FiberId != fiberId);
        }

        public static Fiber GetAnotherFiberOfAdjustmentPoint(this Model model, Guid adjustmentPointId, Guid fiberId)
        {
            return model.Fibers.First(f => (f.NodeId1 == adjustmentPointId || f.NodeId2 == adjustmentPointId) && f.FiberId != fiberId);
        }
        
        public static bool IsAdjustmentPoint(this Model model, Guid nodeId)
        {
            return model.Equipments.FirstOrDefault(e =>
                       e.NodeId == nodeId && e.Type == EquipmentType.AdjustmentPoint) != null;
        }

        // returns true if there's a fiber between start and finish or they are separated by adjustment points only
        public static bool HasDirectFiberDontMindPoints(this Model model, Guid start, Guid finish)
        {
            foreach (var neighbourNodeId in model.Fibers.Where(f => f.NodeId1 == start || f.NodeId2 == start)
                .Select(n => n.NodeId1 == start ? n.NodeId2 : n.NodeId1))
            {
                var previousNodeId = start;
                var currentNodeId = neighbourNodeId;

                while (true)
                {
                    if (currentNodeId == finish) return true;
                    if (!model.IsAdjustmentPoint(currentNodeId)) break;

                    var fiber = model.Fibers.First(f => f.NodeId1 == currentNodeId && f.NodeId2 != previousNodeId
                                                        || f.NodeId2 == currentNodeId && f.NodeId1 != previousNodeId);
                    previousNodeId = currentNodeId;
                    currentNodeId = fiber.NodeId1 == currentNodeId ? fiber.NodeId2 : fiber.NodeId1;
                }
            }

            return false;
        }
    }
}