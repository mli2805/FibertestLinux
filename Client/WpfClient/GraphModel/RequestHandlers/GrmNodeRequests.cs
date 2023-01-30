using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class GrmNodeRequests
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly Model _model;

        public GrmNodeRequests(IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager, Model model)
        {
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _model = model;
        }

        public async Task MoveNode(MoveNode cmd)
        {
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task AddNodeIntoFiber(RequestAddNodeIntoFiber request)
        {
            var cmd = await PrepareAddNodeIntoFiber(request);
            if (cmd == null)
                return;
            var message = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
                await _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, @"Graph AddNodeIntoFiber: " + message));
        }

        private async Task<AddNodeIntoFiber?> PrepareAddNodeIntoFiber(RequestAddNodeIntoFiber request)
        {
            var tracesPassingFiber = _model.GetTracesPassingFiber(request.FiberId);
            if (request.InjectionType != EquipmentType.AdjustmentPoint && tracesPassingFiber.Any(t=>t.HasAnyBaseRef))
            {
                await _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram));
                return null;
            }

            return new AddNodeIntoFiber()
            {
                Id = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                Position = request.Position,
                InjectionType = request.InjectionType,
                FiberId = request.FiberId,
                NewFiberId1 = Guid.NewGuid(),
                NewFiberId2 = Guid.NewGuid()
            };
        }

        public bool IsFiberContainedInAnyTraceWithBase(Guid fiberId)
        {
            return _model.GetTracesPassingFiber(fiberId).Any(t => t.HasAnyBaseRef);
        }

        public async Task RemoveNode(Guid nodeId, EquipmentType type)
        {
            if (!IsRemoveThisNodePermitted(nodeId, type)) return;
            if (await HasProblems(nodeId)) return;

            var detoursForGraph = new List<NodeDetour>();
            foreach (var trace in _model.Traces.Where(t => t.NodeIds.Contains(nodeId)))
            {
                AddDetoursForTrace(nodeId, trace, detoursForGraph);
            }
            var cmd = new RemoveNode { NodeId = nodeId, IsAdjustmentPoint = type == EquipmentType.AdjustmentPoint, DetoursForGraph = detoursForGraph };

            if (detoursForGraph.Count == 0 && type == EquipmentType.AdjustmentPoint)
                cmd.FiberIdToDetourAdjustmentPoint = Guid.NewGuid();

            var message = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
                await _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, @"Graph RemoveNode: " + message));
        }

        // if node has 3 or more neighbours (it's a fork) and one or more from them are adjustment point 
        // or 
        // if it is a U-turn in trace and previous node in trace is adjustment point
        // then
        // those adjustment points near this node should be removed before node removal
        // or
        // empty nodes could be added between them and node to remove
        private async Task<bool> HasProblems(Guid nodeId)
        {
            var hasProblems = IsForkWithAdjustmentPointsNearby(nodeId) || IsUturnWithAdjustmentPointsNearby(nodeId);
            if (!hasProblems) return false;

            var strs = new List<string> {
                Resources.SID_Remove_adjustment_points_or_add_nodes,
                Resources.SID_next_to_the_node_your_are_going_to_remove
            };
            var vm = new MyMessageBoxViewModel(MessageType.Information, strs, -1);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            return true;
        }

        private bool IsUturnWithAdjustmentPointsNearby(Guid nodeId)
        {
            foreach (var trace in _model.Traces.Where(t=>t.NodeIds.Contains(nodeId)))
            {
                for (int i = 0; i < trace.NodeIds.Count-1; i++)
                {
                    if (trace.NodeIds[i] != nodeId) continue;
                    if (trace.NodeIds[i - 1] == trace.NodeIds[i + 1] &&
                        _model.Nodes.First(n=>n.NodeId == trace.NodeIds[i-1]).TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint) return true;
                }
            }
            return false;
        }
        private bool IsForkWithAdjustmentPointsNearby(Guid nodeId)
        {
            var neighbourIds = _model.Fibers.Where(f => f.NodeId1 == nodeId || f.NodeId2 == nodeId)
                .Select(f => f.NodeId1 == nodeId ? f.NodeId2 : f.NodeId1).ToList();
            if (neighbourIds.Count <= 2) return false;

            foreach (var neighbourId in neighbourIds)
            {
                var neighbour = _model.Nodes.Single(n => n.NodeId == neighbourId);
                if (neighbour.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
                    return true;
            }

            // there are no adjustment points around
            return false;
        }

        private void AddDetoursForTrace(Guid nodeId, Trace trace, List<NodeDetour> alreadyMadeDetours)
        {
            for (int i = 1; i < trace.NodeIds.Count; i++)
            {
                if (trace.NodeIds[i] != nodeId) continue;
                var detour = new NodeDetour()
                {
                    FiberId = Guid.NewGuid(), // if there is a fiber between NodeId1 and NodeId2 already - new fiberId just won't be used
                    NodeId1 = trace.NodeIds[i - 1],
                    NodeId2 = trace.NodeIds[i + 1],
                    TraceState = trace.State,
                    TraceId = trace.TraceId,
                };
                alreadyMadeDetours.Add(detour);
            }
        }

        private bool IsRemoveThisNodePermitted(Guid nodeId, EquipmentType type)
        {
            if (_model.Traces.Any(t => t.NodeIds.Contains(nodeId) && t.HasAnyBaseRef) && type != EquipmentType.AdjustmentPoint)
                return false;
            return _model.Traces.All(t => t.NodeIds.Last() != nodeId);
        }
    }
}