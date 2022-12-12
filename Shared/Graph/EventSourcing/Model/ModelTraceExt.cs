using Fibertest.Dto;

namespace Fibertest.Graph
{
    public static class ModelTraceExt
    {
        public static IEnumerable<Guid> GetFibersAtTraceCreation(this Model model, List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
            {
                var fiber = model.Fibers.FirstOrDefault(
                    f => f.NodeId1 == nodes[i - 1] && f.NodeId2 == nodes[i] ||
                         f.NodeId1 == nodes[i] && f.NodeId2 == nodes[i - 1]);

                if (fiber != null)
                    yield return fiber.FiberId;
            }
        }

        public static IEnumerable<Trace> GetTracesPassingFiber(this Model model, Guid fiberId)
        {
            var fiber = model.Fibers.FirstOrDefault(f => f.FiberId == fiberId);
            if (fiber == null) yield break;
            foreach (var pair in fiber.States)
                yield return model.Traces.First(t => t.TraceId == pair.Key);
        }

        // on graph could be more than one fiber between landmarks
        // so we should exclude AdjustmentPoints to find nodes corresponding to landmarks
        // and return all fibers between those nodes
        public static IEnumerable<Guid> GetTraceFibersBetweenLandmarks(this Model model, Guid traceId, int leftLmIndex, int rightLmIndex)
        {
            if (leftLmIndex == -1 || rightLmIndex == -1)
            {
                yield break;
            }
            var trace = model.Traces.First(t => t.TraceId == traceId);
            var nodesWithoutAdjustmentPoints = model.GetTraceNodesExcludingAdjustmentPoints(traceId).ToList();
            var leftNodeId = nodesWithoutAdjustmentPoints[leftLmIndex];
            var rightNodeId = nodesWithoutAdjustmentPoints[rightLmIndex];

            var leftNodeIndexInFull = trace.NodeIds.IndexOf(leftNodeId);
            var rightNodeIndexInFull = trace.NodeIds.IndexOf(rightNodeId);

            for (int i = leftNodeIndexInFull; i < rightNodeIndexInFull; i++)
            {
                yield return model.Fibers.First(
                    f => f.NodeId1 == trace.NodeIds[i] && f.NodeId2 == trace.NodeIds[i + 1] ||
                         f.NodeId1 == trace.NodeIds[i + 1] && f.NodeId2 == trace.NodeIds[i]).FiberId;

            }
        }

        public static void GetTraceForAccidentDefine(this Model model, Guid traceId,
            out Rtu rtu, out List<Node> nodes, out List<Equipment> equipments)
        {
            var trace = model.Traces.First(t => t.TraceId == traceId);
            rtu = model.Rtus.First(r => r.Id == trace.RtuId);
            nodes = new List<Node>();
            foreach (var traceNodeId in trace.NodeIds)
            {
                var node = model.Nodes.First(n => n.NodeId == traceNodeId);
                if (node.TypeOfLastAddedEquipment != EquipmentType.AdjustmentPoint)
                    nodes.Add(node);
            }
            equipments = new List<Equipment>();
            foreach (var equipmentId in trace.EquipmentIds.Skip(1)) // 0 - RTU
            {
                var equipment = model.Equipments.First(e => e.EquipmentId == equipmentId);
                if (equipment.Type != EquipmentType.AdjustmentPoint)
                    equipments.Add(equipment);
            }
        }
    }
}