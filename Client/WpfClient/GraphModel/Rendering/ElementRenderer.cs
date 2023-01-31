using System;
using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public static class ElementRenderer
    {
        public static NodeVm Map(Node node)
        {
            return new NodeVm()
            {
                Id = node.NodeId,
                Title = node.Title,
                Position = node.Position,
                Type = node.TypeOfLastAddedEquipment,
                AccidentOnTraceVmId = node.AccidentOnTraceId,
                State = node.State,
                IsHighlighted = node.IsHighlighted,
            };
        }

        public static FiberVm? Map(Fiber fiber, List<NodeVm> nodesForRendering)
        {
            var nodeVm1 = nodesForRendering.FirstOrDefault(n => n.Id == fiber.NodeId1);
            var nodeVm2 = nodesForRendering.FirstOrDefault(n => n.Id == fiber.NodeId2);
            if (nodeVm1 == null || nodeVm2 == null) return null;
            return new FiberVm(fiber.FiberId, nodeVm1, nodeVm2)
            {
                States = new Dictionary<Guid, FiberState>(),
                TracesWithExceededLossCoeff = new Dictionary<Guid, FiberState>(),
            };
        }

        public static FiberVm? MapWithStates(Fiber fiber, IEnumerable<NodeVm> nodesForRendering)
        {
            var fiberVm = Map(fiber, nodesForRendering.ToList());
            if (fiberVm == null)
                return null;
            foreach (var pair in fiber.States)
                fiberVm.States.Add(pair.Key, pair.Value);
            foreach (var pair in fiber.TracesWithExceededLossCoeff)
                fiberVm.TracesWithExceededLossCoeff.Add(pair.Key, pair.Value);
            return fiberVm;
        }  
        
        public static FiberVm MapWithStates(Fiber fiber, NodeVm nodeVm1, NodeVm nodeVm2)
        {
            var fiberVm = new FiberVm(fiber.FiberId, nodeVm1, nodeVm2)
            {
                HighLights = new List<Guid>(fiber.HighLights ?? new List<Guid>()),
                States = new Dictionary<Guid, FiberState>(),
                TracesWithExceededLossCoeff = new Dictionary<Guid, FiberState>(),
            };
            foreach (var pair in fiber.States)
                fiberVm.States.Add(pair.Key, pair.Value);
            foreach (var pair in fiber.TracesWithExceededLossCoeff)
                fiberVm.TracesWithExceededLossCoeff.Add(pair.Key, pair.Value);
            return fiberVm;
        }
    }
}