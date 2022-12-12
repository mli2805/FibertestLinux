using AutoMapper;
using Fibertest.Dto;

namespace Fibertest.Graph
{
    public static class NodeEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();


        public static string? AddNodeIntoFiber(this Model model, NodeIntoFiberAdded e)
        {
            var oldFiber = model.Fibers.FirstOrDefault(f => f.FiberId == e.FiberId);
            if (oldFiber == null)
            {
                return $@"NodeIntoFiberAdded: Fiber {e.FiberId.First6()} not found";
            }

            model.Nodes.Add(new Node() { NodeId = e.Id, Position = e.Position, TypeOfLastAddedEquipment = e.InjectionType });
            model.Equipments.Add(new Equipment() { EquipmentId = e.EquipmentId, Type = e.InjectionType, NodeId = e.Id });

            model.CreateTwoFibers(e, oldFiber);
            model.FixTracesPassingOldFiber(e, oldFiber);

            model.Fibers.Remove(oldFiber);
            return null;
        }

        private static void FixTracesPassingOldFiber(this Model model, NodeIntoFiberAdded e, Fiber oldFiber)
        {
            foreach (var traceId in oldFiber.States.Keys)
            {
                var trace = model.Traces.First(t => t.TraceId == traceId);

                var oldNodesArray = trace.NodeIds.ToArray();
                var oldFibersArray = trace.FiberIds.ToArray();
                var oldEquipmentsArray = trace.EquipmentIds.ToArray();

                trace.FiberIds.Clear();
                trace.NodeIds.Clear(); trace.NodeIds.Add(oldNodesArray[0]);
                trace.EquipmentIds.Clear(); trace.EquipmentIds.Add(oldEquipmentsArray[0]);

                for (int i = 0; i < oldFibersArray.Length; i++)
                {
                    if (oldFibersArray[i] != oldFiber.FiberId)
                    {
                        trace.FiberIds.Add(oldFibersArray[i]);
                        trace.NodeIds.Add(oldNodesArray[i + 1]);
                        trace.EquipmentIds.Add(oldEquipmentsArray[i + 1]);
                    }
                    else
                    {
                        trace.FiberIds.Add(model.GetOneOfNewFibersId(e, trace.NodeIds.Last()));
                        trace.FiberIds.Add(trace.FiberIds.Last() == e.NewFiberId1 ? e.NewFiberId2 : e.NewFiberId1);
                        trace.NodeIds.Add(e.Id);
                        trace.NodeIds.Add(oldNodesArray[i + 1]);
                        trace.EquipmentIds.Add(e.EquipmentId);
                        trace.EquipmentIds.Add(oldEquipmentsArray[i + 1]);
                    }
                }
            }
        }

        private static Guid GetOneOfNewFibersId(this Model model, NodeIntoFiberAdded e, Guid nodeNearby)
        {
            var fiber1 = model.Fibers.First(f => f.FiberId == e.NewFiberId1);
            return fiber1.NodeId1 == nodeNearby || fiber1.NodeId2 == nodeNearby ? e.NewFiberId1 : e.NewFiberId2;
        }



        private static void CreateTwoFibers(this Model model, NodeIntoFiberAdded e, Fiber oldFiber)
        {
            Guid nodeId1 = model.Fibers.First(f => f.FiberId == e.FiberId).NodeId1;
            Guid nodeId2 = model.Fibers.First(f => f.FiberId == e.FiberId).NodeId2;

            var fiber1 = new Fiber() { FiberId = e.NewFiberId1, NodeId1 = nodeId1, NodeId2 = e.Id };
            foreach (var pair in oldFiber.States)
                fiber1.States.Add(pair.Key, pair.Value);
            foreach (var pair in oldFiber.TracesWithExceededLossCoeff)
                fiber1.TracesWithExceededLossCoeff.Add(pair.Key, pair.Value);
            model.Fibers.Add(fiber1);

            var fiber2 = new Fiber() { FiberId = e.NewFiberId2, NodeId1 = e.Id, NodeId2 = nodeId2 };
            foreach (var pair in oldFiber.States)
                fiber2.States.Add(pair.Key, pair.Value);
            foreach (var pair in oldFiber.TracesWithExceededLossCoeff)
                fiber2.TracesWithExceededLossCoeff.Add(pair.Key, pair.Value);
            model.Fibers.Add(fiber2);
        }

        public static string? UpdateNode(this Model model, NodeUpdated source)
        {
            Node? destination = model.Nodes.FirstOrDefault(n => n.NodeId == source.NodeId);
            if (destination == null)
            {
                return $@"NodeUpdated: Node {source.NodeId.First6()} not found";
            }
            Mapper.Map(source, destination);
            return null;
        }

        public static string? UpdateAndMoveNode(this Model model, NodeUpdatedAndMoved source)
        {
            Node? destination = model.Nodes.FirstOrDefault(n => n.NodeId == source.NodeId);
            if (destination == null)
            {
                return $@"NodeUpdated: Node {source.NodeId.First6()} not found";
            }
            Mapper.Map(source, destination);
            return null;
        }

        public static string? MoveNode(this Model model, NodeMoved newLocation)
        {
            Node? node = model.Nodes.FirstOrDefault(n => n.NodeId == newLocation.NodeId);
            if (node == null)
            {
                return $@"NodeMoved: Node {newLocation.NodeId.First6()} not found";
            }
            node.Position = new PointLatLng(newLocation.Latitude, newLocation.Longitude);
            return null;
        }

        public static string? RemoveUnused(this Model model)
        {
            var allTracesNodes = new HashSet<Guid>();
            var allTracesFibers = new HashSet<Guid>();
            foreach (var trace in model.Traces)
            {
                allTracesNodes.UnionWith(trace.NodeIds);
                allTracesFibers.UnionWith(trace.FiberIds);
            }

            for (int i = model.Fibers.Count - 1; i >= 0; i--)
            {
                if (!allTracesFibers.Contains(model.Fibers[i].FiberId))
                    model.Fibers.Remove(model.Fibers[i]);
            }

            for (int i = model.Nodes.Count - 1; i >= 0; i--)
            {
                if (!allTracesNodes.Contains(model.Nodes[i].NodeId)                  // not in traces
                    && model.Nodes[i].AccidentOnTraceId == Guid.Empty                // not an accident
                    && model.Rtus.All(r => r.NodeId != model.Nodes[i].NodeId))   // not an RTU
                {
                    for (int j = model.Equipments.Count - 1; j >= 0; j--)
                    {
                        if (model.Equipments[j].NodeId == model.Nodes[i].NodeId)
                            model.Equipments.Remove(model.Equipments[j]);
                    }
                    model.Nodes.Remove(model.Nodes[i]);
                }
            }

            return null;
        }

        public static string? RemoveNode(this Model model, NodeRemoved e)
        {
            foreach (var trace in model.Traces.Where(t => t.NodeIds.Contains(e.NodeId)))
            {
                var result = model.ExcludeAllNodeAppearancesInTrace(e.NodeId, trace, e.DetoursForGraph);
                if (result != null) return result;
            }

            if (e.FiberIdToDetourAdjustmentPoint != Guid.Empty)
                return model.ExcludeAdjustmentPoint(e.NodeId, e.FiberIdToDetourAdjustmentPoint);

            return e.DetoursForGraph.Count == 0
                ? model.RemoveNodeWithAllHisFibersUptoRealNode(e.NodeId)
                : model.RemoveNodeWithAllHisFibers(e.NodeId);
        }

        private static string? ExcludeAllNodeAppearancesInTrace(this Model model, Guid nodeId, Trace trace, List<NodeDetour> nodeDetours)
        {
            int index;
            while ((index = trace.NodeIds.IndexOf(nodeId)) != -1)
            {
                var detour = nodeDetours.FirstOrDefault(d =>
                    d.TraceId == trace.TraceId && d.NodeId1 == trace.NodeIds[index - 1] &&
                    d.NodeId2 == trace.NodeIds[index + 1]);
                if (detour == null)
                {
                    return $@"NodeRemoved: No fiber prepared to detour trace {trace.TraceId}";
                }

                if (detour.NodeId1 == detour.NodeId2)
                {
                    var result = ExcludeTurnNodeFromTrace(trace, index);
                    if (result != null) return result;
                }
                else
                {
                    model.CreateDetourIfAbsent(detour);
                    trace.EquipmentIds.RemoveAt(index);
                    trace.NodeIds.RemoveAt(index);

                    trace.FiberIds.RemoveAt(index);
                    trace.FiberIds.RemoveAt(index - 1);

                    // there is a new detour fiber after `model.CreateDetourIfAbsent(detour);` so use it.
                    var fiberId = model.GetFiberIdBetweenNodes(detour.NodeId1, detour.NodeId2);
                    trace.FiberIds.Insert(index - 1, fiberId);
                }
            }

            return null;
        }

        private static string? ExcludeTurnNodeFromTrace(Trace trace, int index)
        {
            if (trace.NodeIds.Count <= 3)
            {
                return $@"NodeRemoved: Trace {trace.TraceId} is too short to remove turn node";
            }
            if (index == 1) index++;

            trace.EquipmentIds.RemoveAt(index);
            trace.NodeIds.RemoveAt(index);
            trace.FiberIds.RemoveAt(index);
            trace.EquipmentIds.RemoveAt(index - 1);
            trace.NodeIds.RemoveAt(index - 1);
            trace.FiberIds.RemoveAt(index - 1);

            return null;
        }

        private static void CreateDetourIfAbsent(this Model model, NodeDetour detour)
        {
            var nodeBefore = detour.NodeId1;
            var nodeAfter = detour.NodeId2;

            var fiber = model.Fibers.FirstOrDefault(f => f.NodeId1 == nodeBefore && f.NodeId2 == nodeAfter
                                                        || f.NodeId2 == nodeBefore && f.NodeId1 == nodeAfter);
            if (fiber == null)
            {
                fiber = new Fiber()
                {
                    FiberId = detour.FiberId,
                    NodeId1 = nodeBefore,
                    NodeId2 = nodeAfter,
                    States = new Dictionary<Guid, FiberState> { { detour.TraceId, detour.TraceState } }
                };
                model.Fibers.Add(fiber);
            }
            else
            {
                if (!fiber.States.ContainsKey(detour.TraceId))
                    fiber.States.Add(detour.TraceId, detour.TraceState);
            }

        }

        private static string? ExcludeAdjustmentPoint(this Model model, Guid nodeId, Guid detourFiberId)
        {
            var leftFiber = model.Fibers.FirstOrDefault(f => f.NodeId2 == nodeId);
            if (leftFiber == null)
            {
                return @"IsFiberContainedInAnyTraceWithBase: Left fiber not found";
            }
            var leftNodeId = leftFiber.NodeId1;

            var rightFiber = model.Fibers.FirstOrDefault(f => f.NodeId1 == nodeId);
            if (rightFiber == null)
            {
                return @"IsFiberContainedInAnyTraceWithBase: Right fiber not found";
            }
            var rightNodeId = rightFiber.NodeId2;

            model.Fibers.Remove(leftFiber);
            model.Fibers.Remove(rightFiber);
            model.Fibers.Add(new Fiber() { FiberId = detourFiberId, NodeId1 = leftNodeId, NodeId2 = rightNodeId });

            var node = model.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (node == null)
            {
                return $@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found";
            }

            model.Nodes.Remove(node);
            return null;
        }
    }
}