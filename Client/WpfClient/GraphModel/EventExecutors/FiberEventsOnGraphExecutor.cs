using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class FiberEventsOnGraphExecutor
    {
        private readonly GraphReadModel _graphModel;
        private readonly CurrentUser _currentUser;

        public FiberEventsOnGraphExecutor(GraphReadModel graphModel, CurrentUser currentUser)
        {
            _graphModel = graphModel;
            _currentUser = currentUser;
        }

        public void AddFiber(FiberAdded evnt)
        {
            if (_currentUser.Role > Role.Root) return;

            _graphModel.Data.Fibers.Add(new FiberVm(evnt.FiberId,
                _graphModel.Data.Nodes.First(m => m.Id == evnt.NodeId1),
                 _graphModel.Data.Nodes.First(m => m.Id == evnt.NodeId2)));
        }

        public void RemoveFiber(FiberRemoved evnt)
        {
            //            if (_currentUser.ZoneId != Guid.Empty
            //                && _graphModel.Data.Fibers.All(f => f.Id != evnt.FiberId)) return;

            var fiberVm = _graphModel.Data.Fibers.FirstOrDefault(f => f.Id == evnt.FiberId);
            if (fiberVm != null)
                RemoveFiberUptoRealNodesNotPoints(fiberVm);
        }

        private void RemoveFiberUptoRealNodesNotPoints(FiberVm fiber)
        {
            var leftNode = _graphModel.Data.Nodes.First(n => n.Id == fiber.Node1.Id);
            while (leftNode.Type == EquipmentType.AdjustmentPoint)
            {
                var leftFiber = _graphModel.GetAnotherFiberOfAdjustmentPoint(leftNode, fiber.Id);
                _graphModel.Data.Nodes.Remove(leftNode);
                var nextLeftNodeId = leftFiber.Node1.Id == leftNode.Id ? leftFiber.Node2.Id : leftFiber.Node1.Id;
                _graphModel.Data.Fibers.Remove(leftFiber);
                leftNode = _graphModel.Data.Nodes.First(n => n.Id == nextLeftNodeId);
            }

            var rightNode = _graphModel.Data.Nodes.First(n => n.Id == fiber.Node2.Id);
            while (rightNode.Type == EquipmentType.AdjustmentPoint)
            {
                var rightFiber = _graphModel.GetAnotherFiberOfAdjustmentPoint(rightNode, fiber.Id);
                _graphModel.Data.Nodes.Remove(rightNode);
                var nextRightNodeId = rightFiber.Node1.Id == rightNode.Id ? rightFiber.Node2.Id : rightFiber.Node1.Id;
                _graphModel.Data.Fibers.Remove(rightFiber);
                rightNode = _graphModel.Data.Nodes.First(n => n.Id == nextRightNodeId);
            }

            _graphModel.Data.Fibers.Remove(fiber);
        }


    }
}