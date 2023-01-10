using Fibertest.Dto;

namespace Fibertest.Graph;

public static class ModelTraceDefinitionExt
{
    // start and finish are NOT included
    public static bool FindPathWhereAdjustmentPointsOnly(this Model model, Guid start, Guid finish,
        out List<Guid> pathNodeIds)
    {
        pathNodeIds = new List<Guid>();

        foreach (var neighbourId in model.GetNodeNeighbours(start))
        {
            pathNodeIds.Clear();
            var previousNodeId = start;
            var currentNodeId = neighbourId;

            while (true)
            {
                if (currentNodeId == finish) return true;
                var currentNode = model.Nodes.First(n => n.NodeId == currentNodeId);
                if (currentNode.TypeOfLastAddedEquipment != EquipmentType.AdjustmentPoint) break;

                pathNodeIds.Add(currentNodeId);

                var fiber = model.Fibers.First(f =>
                    f.NodeId1 == currentNodeId && f.NodeId2 != previousNodeId ||
                    f.NodeId2 == currentNodeId && f.NodeId1 != previousNodeId);
                previousNodeId = currentNodeId;
                currentNodeId = fiber.NodeId1 == currentNodeId ? fiber.NodeId2 : fiber.NodeId1;
            }
        }

        return false;
    }

    // if some of neighbors are AdjustmentPoints - step farther a find first node on this way
    // RTU could not be inside trace
    public static List<Tuple<Guid, List<Guid>>> GetNeighboursPassingThroughAdjustmentPoints(this Model model,
        Guid nodeId)
    {
        var res = new List<Tuple<Guid, List<Guid>>>();

        var node = model.Nodes.First(n => n.NodeId == nodeId);
        var fibers = model.GetNodeFibers(node);
        foreach (var fiber in fibers)
        {
            var fiberIdsOfOneDestination = new List<Guid>();
            var previousNodeId = nodeId;
            var currentFiber = fiber;
            Guid neighbourId;
            Node neighbour;
            while (true)
            {
                fiberIdsOfOneDestination.Add(currentFiber.FiberId);
                neighbourId = currentFiber.NodeId1 == previousNodeId ? currentFiber.NodeId2 : currentFiber.NodeId1;
                neighbour = model.Nodes.First(n => n.NodeId == neighbourId);
                if (neighbour.TypeOfLastAddedEquipment != EquipmentType.AdjustmentPoint)
                    break;

                previousNodeId = neighbourId;
                currentFiber = model.GetAnotherFiberOfAdjustmentPoint(neighbourId, currentFiber.FiberId);
            }
            if (neighbour.TypeOfLastAddedEquipment != EquipmentType.Rtu)
                res.Add(new Tuple<Guid, List<Guid>>(neighbourId, fiberIdsOfOneDestination));
        }
        return res;
    }
}