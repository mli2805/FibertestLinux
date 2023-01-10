using Fibertest.Dto;

namespace Fibertest.Graph;

public static class ModelRemoveThroughAdjustmentPointsExt
{
    public static string? RemoveNodeWithAllHisFibersUptoRealNode(this Model model, Guid nodeId)
    {
        foreach (var fiber in model.Fibers.Where(f => f.NodeId1 == nodeId || f.NodeId2 == nodeId).ToList())
        {
            var fiberForDeletion = fiber;
            var nodeForDeletionId = nodeId;
            while (true)
            {
                var anotherNodeId = fiberForDeletion.NodeId1 == nodeForDeletionId
                    ? fiberForDeletion.NodeId2
                    : fiberForDeletion.NodeId1;
                model.Fibers.Remove(fiberForDeletion);
                if (!model.IsAdjustmentPoint(anotherNodeId)) break;

                fiberForDeletion =
                    model.Fibers.First(f => f.NodeId1 == anotherNodeId || f.NodeId2 == anotherNodeId);
                model.Nodes.RemoveAll(n => n.NodeId == anotherNodeId);
                model.Equipments.RemoveAll(e => e.NodeId == anotherNodeId);
                nodeForDeletionId = anotherNodeId;
            }
        }

        model.Equipments.RemoveAll(e => e.NodeId == nodeId);
        model.Nodes.RemoveAll(n => n.NodeId == nodeId);
        return null;
    }

    public static string? RemoveNodeWithAllHisFibers(this Model model, Guid nodeId)
    {
        model.Fibers.RemoveAll(f => f.NodeId1 == nodeId || f.NodeId2 == nodeId);
        model.Equipments.RemoveAll(e => e.NodeId == nodeId);
        model.Nodes.RemoveAll(n => n.NodeId == nodeId);
        return null;
    }

    public static void RemoveFiberUptoRealNodesNotPoints(this Model model, Fiber fiber)
    {
        var leftNode = model.Nodes.First(n => n.NodeId == fiber.NodeId1);
        while (leftNode.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
        {
            var leftFiber = model.GetAnotherFiberOfAdjustmentPoint(leftNode, fiber.FiberId);
            model.Nodes.Remove(leftNode);
            var nextLeftNodeId = leftFiber.NodeId1 == leftNode.NodeId ? leftFiber.NodeId2 : leftFiber.NodeId1;
            model.Fibers.Remove(leftFiber);
            leftNode = model.Nodes.First(n => n.NodeId == nextLeftNodeId);
        }

        var rightNode = model.Nodes.First(n => n.NodeId == fiber.NodeId2);
        while (rightNode.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
        {
            var rightFiber = model.GetAnotherFiberOfAdjustmentPoint(rightNode, fiber.FiberId);
            model.Nodes.Remove(rightNode);
            var nextRightNodeId = rightFiber.NodeId1 == rightNode.NodeId ? rightFiber.NodeId2 : rightFiber.NodeId1;
            model.Fibers.Remove(rightFiber);
            rightNode = model.Nodes.First(n => n.NodeId == nextRightNodeId);
        }

        model.Fibers.Remove(fiber);
    }
}