using Fibertest.Dto;

namespace Fibertest.Graph
{
    public static class ModelTraceComponentsExt
    {
        public static TraceModelForBaseRef GetTraceComponentsByIds(this Model model, Trace trace)
        {
            var nodes = model.GetTraceNodes(trace).ToArray();
            var rtu = model.Rtus.First(r => r.Id == trace.RtuId);
            var equipList =
                new List<Equipment>()
                {
                    new Equipment() {Type = EquipmentType.Rtu, Title = rtu.Title}
                }; // fake RTU, just for indexes match
            equipList.AddRange(model.GetTraceEquipments(trace).ToList()); // without RTU
            var fibers = model.GetTraceFibers(trace).ToArray();

            return new TraceModelForBaseRef(nodes, equipList.ToArray(), fibers);
        }

        private static IEnumerable<Node> GetTraceNodes(this Model model, Trace trace)
        {
            return trace.NodeIds.Select(i => model.Nodes.First(eq => eq.NodeId == i));
        }

        public static IEnumerable<Equipment> GetTraceEquipments(this Model model, Trace trace)
        {
            return trace.EquipmentIds.Skip(1).Select(i => model.Equipments.Single(eq => eq.EquipmentId == i));
            // return trace.EquipmentIds.Skip(1).Select(i => model.Equipments.First(eq => eq.EquipmentId == i));
        }

        public static IEnumerable<Fiber> GetTraceFibers(this Model model, Trace trace)
        {
            foreach (var fiberId in trace.FiberIds)
            {
                var fiber = model.Fibers.FirstOrDefault(f => f.FiberId == fiberId);
                if (fiber != null)
                    yield return fiber;
            }
        }

        public static IEnumerable<Guid> GetTraceNodesExcludingAdjustmentPoints(this Model model, Guid traceId)
        {
            var trace = model.Traces.First(t => t.TraceId == traceId);
            foreach (var nodeId in trace.NodeIds)
            {
                var node = model.Nodes.FirstOrDefault(n =>
                    n.NodeId == nodeId && n.TypeOfLastAddedEquipment != EquipmentType.AdjustmentPoint);
                if (node != null)
                    yield return node.NodeId;
            }
        }

        public static IEnumerable<Equipment> GetTraceEquipmentsExcludingAdjustmentPoints(this Model model, Guid traceId)
        {
            var trace = model.Traces.First(t => t.TraceId == traceId);
            foreach (var equipmentId in trace.EquipmentIds.Skip(1)) // 0 - RTU
            {
                var equipment = model.Equipments.First(e => e.EquipmentId == equipmentId);
                if (equipment.Type != EquipmentType.AdjustmentPoint)
                    yield return equipment;
            }
        }
    }
}