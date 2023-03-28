using Fibertest.Dto;

namespace Fibertest.Graph
{
    public class GraphGpsCalculator
    {
        private readonly Model _model;

        public GraphGpsCalculator(Model model)
        {
            _model = model;
        }

        public double CalculateTraceGpsLengthKm2(Trace trace)
        {
            double result = 0;

            for (int i = 0; i < trace.FiberIds.Count; i++)
            {
                var fiber = _model.Fibers.FirstOrDefault(f => f.FiberId == trace.FiberIds[i]);
                if (fiber == null) return 0;

                var nodeA = _model.Nodes.FirstOrDefault(n => n.NodeId == trace.NodeIds[i]);
                if (nodeA == null) return 0;
                var equipmentA = i == 0
                    ? new Equipment() { Type = EquipmentType.Rtu, CableReserveLeft = 0, CableReserveRight = 0 }
                    : _model.Equipments.FirstOrDefault(e => e.EquipmentId == trace.EquipmentIds[i]);
                if (equipmentA == null) return 0;

                var nodeB = _model.Nodes.FirstOrDefault(n => n.NodeId == trace.NodeIds[i + 1]);
                if (nodeB == null) return 0;
                var equipmentB = _model.Equipments.FirstOrDefault(e => e.EquipmentId == trace.EquipmentIds[i + 1]);
                if (equipmentB == null) return 0;

                var distance = GisLabCalculator.GetDistanceBetweenPointLatLng(nodeA.Position, nodeB.Position);
                result += distance;

            }

            return result / 1000;
        }

        public int CalculateDistanceBetweenNodesMm(Node leftNode, Equipment leftEquipment, Node rightNode, Equipment rightEquipment)
        {
            var gpsDistance = (int)GisLabCalculator.GetDistanceBetweenPointLatLng(leftNode.Position, rightNode.Position);
            
            // cable reserve is not a GPS 
            // return (int)((gpsDistance + GetReserveFromTheLeft(leftEquipment) + GetReserveFromTheRight(rightEquipment)) * 1000);
            return gpsDistance * 1000;
        }

        public double GetFiberFullGpsDistance(Guid fiberId, out Node node1, out Node node2)
        {
            var fiber = _model.Fibers.First(f => f.FiberId == fiberId);
            node1 = _model.Nodes.First(n => n.NodeId == fiber.NodeId1);
            node2 = _model.Nodes.First(n => n.NodeId == fiber.NodeId2);
            var result = GisLabCalculator.GetDistanceBetweenPointLatLng(node1.Position, node2.Position);

            var fId = fiberId;
            while (node1.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
            {
                fiber = _model.GetAnotherFiberOfAdjustmentPoint(node1, fId);
                var previousNode1 = node1;
                node1 = _model.Nodes.First(n => n.NodeId == fiber.NodeId1);
                result = result + GisLabCalculator.GetDistanceBetweenPointLatLng(node1.Position, previousNode1.Position);
                fId = fiber.FiberId;
            }

            fId = fiberId;
            while (node2.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
            {
                fiber = _model.GetAnotherFiberOfAdjustmentPoint(node2, fId);
                var previousNode2 = node2;
                node2 = _model.Nodes.First(n => n.NodeId == fiber.NodeId2);
                result += GisLabCalculator.GetDistanceBetweenPointLatLng(node2.Position, previousNode2.Position);
                fId = fiber.FiberId;
            }

            return result;
        }
    }
}