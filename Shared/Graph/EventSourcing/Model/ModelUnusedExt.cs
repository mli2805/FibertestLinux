using Fibertest.Dto;

namespace Fibertest.Graph
{
    public static class ModelUnusedExt
    {
        public static async Task<Tuple<List<Guid>, List<Guid>>> GetUnused(this Model model)
        {
            await Task.Delay(0);
            var allTracesNodes = new HashSet<Guid>();
            var allTracesFibers = new HashSet<Guid>();
            foreach (var trace in model.Traces)
            {
                allTracesNodes.UnionWith(trace.NodeIds);
                allTracesFibers.UnionWith(trace.FiberIds);
            }

            var unusedNodes = new List<Guid>();
            for (int i = 0; i < model.Nodes.Count; i++)
            {
                if (!allTracesNodes.Contains(model.Nodes[i].NodeId)
                    && model.Nodes[i].TypeOfLastAddedEquipment != EquipmentType.Rtu
                    && model.Nodes[i].TypeOfLastAddedEquipment != EquipmentType.AccidentPlace)
                    unusedNodes.Add(model.Nodes[i].NodeId);
            }

            var unusedFibers = new List<Guid>();
            for (int i = 0; i < model.Fibers.Count; i++)
            {
                if (!allTracesFibers.Contains(model.Fibers[i].FiberId))
                    unusedFibers.Add(model.Fibers[i].FiberId);
            }

            return new Tuple<List<Guid>, List<Guid>>(unusedNodes, unusedFibers);
        }

       
    }
}
