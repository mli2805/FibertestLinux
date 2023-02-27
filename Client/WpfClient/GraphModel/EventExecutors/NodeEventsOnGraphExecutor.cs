using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using GMap.NET;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class NodeEventsOnGraphExecutor
    {
        private readonly ILogger _logger;
        private readonly GraphReadModel _graphModel;
        private readonly ILifetimeScope _globalScope;

        public NodeEventsOnGraphExecutor(ILogger logger, GraphReadModel graphModel, ILifetimeScope globalScope)
        {
            _logger = logger;
            _graphModel = graphModel;
            _globalScope = globalScope;
        }

        public void AddNodeIntoFiber(NodeIntoFiberAdded evnt)
        {
            if (_graphModel.Data.Fibers.All(f => f.Id != evnt.FiberId)) return;

            var nodeVm = new NodeVm()
            {
                Id = evnt.Id,
                Position = new PointLatLng(evnt.Position.Lat, evnt.Position.Lng),

                State = FiberState.Ok,
                Type = evnt.InjectionType,

                GraphVisibilityLevelItem = _graphModel.SelectedGraphVisibilityItem,
            };
            _graphModel.Data.Nodes.Add(nodeVm);

            var fiberForDeletion = _graphModel.Data.Fibers.First(f => f.Id == evnt.FiberId);
            AddTwoFibersToNewNode(evnt, fiberForDeletion);
            _graphModel.Data.Fibers.Remove(fiberForDeletion);

            var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
            if (vm.IsOpen)
                vm.AddNodeIntoFiber(evnt);
        }

        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e, FiberVm oldFiberVm)
        {
            NodeVm node1 = _graphModel.Data.Fibers.First(f => f.Id == e.FiberId).Node1;
            NodeVm node2 = _graphModel.Data.Fibers.First(f => f.Id == e.FiberId).Node2;

            var fiberVm1 = new FiberVm(e.NewFiberId1, node1, _graphModel.Data.Nodes.First(n => n.Id == e.Id));
            foreach (var pair in oldFiberVm.States)
                fiberVm1.States.Add(pair.Key, pair.Value);
            foreach (var pair in oldFiberVm.TracesWithExceededLossCoeff)
                fiberVm1.TracesWithExceededLossCoeff.Add(pair.Key, pair.Value);
            fiberVm1.HighLights = new List<Guid>(oldFiberVm.HighLights);
            _graphModel.Data.Fibers.Add(fiberVm1);

            var fiberVm2 = new FiberVm(e.NewFiberId2, _graphModel.Data.Nodes.First(n => n.Id == e.Id), node2);
            foreach (var pair in oldFiberVm.States)
                fiberVm2.States.Add(pair.Key, pair.Value);
            foreach (var pair in oldFiberVm.TracesWithExceededLossCoeff)
                fiberVm2.TracesWithExceededLossCoeff.Add(pair.Key, pair.Value);
            fiberVm2.HighLights = new List<Guid>(oldFiberVm.HighLights);
            _graphModel.Data.Fibers.Add(fiberVm2);
        }

        public void MoveNode(NodeMoved evnt)
        {
            var nodeVm = _graphModel.Data.Nodes.FirstOrDefault(n => n.Id == evnt.NodeId);
            if (nodeVm == null)
                return;
            nodeVm.Position = new PointLatLng(evnt.Latitude, evnt.Longitude);
        }

        public void UpdateNode(NodeUpdated evnt)
        {
            var nodeVm = _graphModel.Data.Nodes.FirstOrDefault(n => n.Id == evnt.NodeId);
            if (nodeVm == null)
                return;
            nodeVm.Title = evnt.Title;

            var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
            if (vm.IsOpen)
                vm.UpdateNode(evnt.NodeId);
        }

        public void UpdateAndMoveNode(NodeUpdatedAndMoved evnt)
        {
            var nodeVm = _graphModel.Data.Nodes.FirstOrDefault(n => n.Id == evnt.NodeId);
            if (nodeVm == null)
                return;
            nodeVm.Title = evnt.Title;
            nodeVm.Position = evnt.Position;
      
            var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
            if (vm.IsOpen)
                vm.UpdateNode(evnt.NodeId);
        }

        public async Task RemoveUnused()
        {
            await _graphModel.RefreshVisiblePart();
        }

        public void RemoveNode(NodeRemoved evnt)
        {
            if (_graphModel.Data.Nodes.All(f => f.Id != evnt.NodeId)) return;

            foreach (var detour in evnt.DetoursForGraph)
                CreateDetourIfAbsent(detour);

            if (evnt.FiberIdToDetourAdjustmentPoint != Guid.Empty)
            {
                ExcludeAdjustmentPoint(evnt.NodeId, evnt.FiberIdToDetourAdjustmentPoint);
                return;
            }

            if (evnt.DetoursForGraph.Count == 0)
                RemoveNodeWithAllHisFibersUptoRealNode(evnt.NodeId);
            else
                RemoveNodeWithAllHisFibers(evnt.NodeId);
        }

        private void ExcludeAdjustmentPoint(Guid nodeId, Guid detourFiberId)
        {
            var leftFiber = _graphModel.Data.Fibers.FirstOrDefault(f => f.Node2.Id == nodeId);
            if (leftFiber == null)
            {
                _logger.Error(Logs.Client,@"IsFiberContainedInAnyTraceWithBase: Left fiber not found");
                return;
            }
            var leftNode = leftFiber.Node1;

            var rightFiber = _graphModel.Data.Fibers.FirstOrDefault(f => f.Node1.Id == nodeId);
            if (rightFiber == null)
            {
                _logger.Error(Logs.Client,@"IsFiberContainedInAnyTraceWithBase: Right fiber not found");
                return;
            }
            var rightNode = rightFiber.Node2;

            _graphModel.Data.Fibers.Remove(leftFiber);
            _graphModel.Data.Fibers.Remove(rightFiber);
            _graphModel.Data.Fibers.Add(new FiberVm(detourFiberId, leftNode, rightNode));

            var node = _graphModel.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null)
            {
                _logger.Error(Logs.Client,$@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found");
                return;
            }

            _graphModel.Data.Nodes.Remove(node);
        }

        private void RemoveNodeWithAllHisFibers(Guid nodeId)
        {
            foreach (var fiber in _graphModel.Data.Fibers.Where(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId).ToList())
                _graphModel.Data.Fibers.Remove(fiber);
            _graphModel.Data.Nodes.Remove(_graphModel.Data.Nodes.First(n => n.Id == nodeId));
        }

        // if AdjustmentPoint encounter it will be deleted and we'll pass farther
        public void RemoveNodeWithAllHisFibersUptoRealNode(Guid nodeId)
        {
            var node = _graphModel.Data.Nodes.FirstOrDefault(f => f.Id == nodeId);
            if (node == null) return;

            foreach (var fiber in _graphModel.Data.Fibers.Where(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId).ToList())
            {
                var fiberForDeletion = fiber;
                var nodeForDeletionId = nodeId;
                while (true)
                {
                    var anotherNode = fiberForDeletion.Node1.Id == nodeForDeletionId ? fiberForDeletion.Node2 : fiberForDeletion.Node1;
                    _graphModel.Data.Fibers.Remove(fiberForDeletion);
                    if (anotherNode.Type != EquipmentType.AdjustmentPoint) break;

                    fiberForDeletion = _graphModel.Data.Fibers.First(f => f.Node1.Id == anotherNode.Id || f.Node2.Id == anotherNode.Id);
                    _graphModel.Data.Nodes.Remove(anotherNode);
                    nodeForDeletionId = anotherNode.Id;
                }
            }

            _graphModel.Data.Nodes.Remove(node);
        }

        private void CreateDetourIfAbsent(NodeDetour detour)
        {
            var fiberVm = _graphModel.Data.Fibers.FirstOrDefault(f => f.Node1.Id == detour.NodeId1 && f.Node2.Id == detour.NodeId2
                                                       || f.Node2.Id == detour.NodeId1 && f.Node1.Id == detour.NodeId2);
            if (fiberVm == null)
            {
                fiberVm = new FiberVm(detour.FiberId,
                    _graphModel.Data.Nodes.First(m => m.Id == detour.NodeId1),
                    _graphModel.Data.Nodes.First(m => m.Id == detour.NodeId2));
                _graphModel.Data.Fibers.Add(fiberVm);
            }
            fiberVm.SetState(detour.TraceId, detour.TraceState);
        }
    }
}