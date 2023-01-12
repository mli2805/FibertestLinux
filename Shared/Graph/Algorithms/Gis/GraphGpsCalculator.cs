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

        public double CalculateTraceGpsLengthKm(Trace trace)
        {
            double result = 0;
            for (int i = 0; i < trace.NodeIds.Count - 1; i++)
            {
                var node1 = _model.Nodes.FirstOrDefault(n => n.NodeId == trace.NodeIds[i]);
                if (node1 == null) return 0;
                var node2 = _model.Nodes.FirstOrDefault(n => n.NodeId == trace.NodeIds[i + 1]);
                if (node2 == null) return 0;

                var equipment1 = i == 0
                    ? new Equipment() { Type = EquipmentType.Rtu, CableReserveLeft = 0, CableReserveRight = 0 }
                    : _model.Equipments.FirstOrDefault(e => e.EquipmentId == trace.EquipmentIds[i]);
                var equipment2 = _model.Equipments.FirstOrDefault(e => e.EquipmentId == trace.EquipmentIds[i + 1]);

                if (equipment1 == null || equipment2 == null)
                    return 0;
                result = result +
                         GisLabCalculator.GetDistanceBetweenPointLatLng(node1.Position, node2.Position) +
                         GetReserveFromTheLeft(equipment1) + GetReserveFromTheRight(equipment2);
            }
            return result / 1000;
        }

        public int CalculateDistanceBetweenNodesMm(Node leftNode, Equipment leftEquipment, Node rightNode, Equipment rightEquipment)
        {
            var gpsDistance = (int)GisLabCalculator.GetDistanceBetweenPointLatLng(leftNode.Position, rightNode.Position);

            return (int)((gpsDistance + GetReserveFromTheLeft(leftEquipment) + GetReserveFromTheRight(rightEquipment)) * 1000);
        }

        private double GetReserveFromTheLeft(Equipment leftEquipment)
        {
            var leftReserve = 0.0;
            if (leftEquipment.Type == EquipmentType.CableReserve)
                leftReserve = (double)leftEquipment.CableReserveLeft / 2;
            else if (leftEquipment.Type > EquipmentType.CableReserve)
                leftReserve = leftEquipment.CableReserveRight;
            return leftReserve;
        }

        private double GetReserveFromTheRight(Equipment rightEquipment)
        {
            var rightReserve = 0.0;
            if (rightEquipment.Type == EquipmentType.CableReserve)
                rightReserve = (double)rightEquipment.CableReserveLeft / 2;
            else if (rightEquipment.Type > EquipmentType.CableReserve)
                rightReserve = rightEquipment.CableReserveLeft;
            return rightReserve;
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
                result = result + GisLabCalculator.GetDistanceBetweenPointLatLng(node2.Position, previousNode2.Position);
                fId = fiber.FiberId;
            }

            return result;
        }
    }
}