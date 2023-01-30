using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class TraceStepByStepViewModel : Screen
    {
        public bool IsOpen { get; set; }

        private readonly ILifetimeScope _globalScope;
        private readonly GraphReadModel _graphReadModel;
        private readonly Model _readModel;
        private readonly StepChoiceViewModel _stepChoiceViewModel;
        private readonly IWindowManager _windowManager;
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;
        private NodeVm _currentHighlightedNodeVm;
        public ObservableCollection<StepModel> Steps { get; set; }
        private Guid _newTraceId;

        private bool _isButtonsEnabled = true;
        public bool IsButtonsEnabled
        {
            get { return _isButtonsEnabled; }
            set
            {
                if (value == _isButtonsEnabled) return;
                _isButtonsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public TraceStepByStepViewModel(ILifetimeScope globalScope, IWindowManager windowManager, CommonStatusBarViewModel commonStatusBarViewModel,
            GraphReadModel graphReadModel, Model readModel, StepChoiceViewModel stepChoiceViewModel)
        {
            _globalScope = globalScope;
            _graphReadModel = graphReadModel;
            _readModel = readModel;
            _stepChoiceViewModel = stepChoiceViewModel;
            _windowManager = windowManager;
            _commonStatusBarViewModel = commonStatusBarViewModel;
        }

        public async Task<int> Initialize(Guid rtuNodeId, string rtuTitle)
        {
            _newTraceId = Guid.NewGuid();
            Steps = new ObservableCollection<StepModel>();

            var rtuNode = _readModel.Nodes.First(n => n.NodeId == rtuNodeId);
            if (_graphReadModel.CurrentGis.ThresholdZoom > _graphReadModel.MainMap.Zoom)
                _graphReadModel.MainMap.Zoom = _graphReadModel.CurrentGis.ThresholdZoom;
            _graphReadModel.MainMap.SetPositionWithoutFiringEvent(rtuNode.Position);

            var nodeCount = await _graphReadModel.RefreshVisiblePart();

            _currentHighlightedNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == rtuNodeId);
            _currentHighlightedNodeVm.IsHighlighted = true;
            var firstStepRtu = new StepModel()
            {
                NodeId = rtuNodeId,
                Title = rtuTitle,
                EquipmentId = _readModel.Rtus.First(r => r.NodeId == rtuNodeId).Id,
                FiberIds = new List<Guid>(), // empty 
            };
            Steps.Add(firstStepRtu);

            return nodeCount;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Step_by_step_trace_defining;
            _graphReadModel.MainMap.IsInTraceDefinitionMode = true;
            IsOpen = true;
        }

        public async void SemiautomaticMode()
        {
            var isButtonPressed = true;
            while (await MakeStepForward(isButtonPressed))
            {
                isButtonPressed = false;
            }
        }

        public async void StepBackward()
        {
            if (Steps.Count == 1) return;
            Guid backwardNodeId = Steps[Steps.Count - 2].NodeId;
            if (_readModel.Rtus.Any(r => r.NodeId == backwardNodeId))
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Trace_cannot_be_terminated_by_or_pass_through_RTU_);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            await JustStep(backwardNodeId, Steps.Last().FiberIds);
        }

        public async Task<bool> StepForward()
        {
            return await MakeStepForward(true);
        }

        private async Task<bool> MakeStepForward(bool isButtonPressed)
        {
            // return a previous node among others
            var neighbours = _readModel.GetNeighboursPassingThroughAdjustmentPoints(Steps.Last().NodeId);
            Guid previousNodeId = Steps.Count == 1 ? Guid.Empty : Steps[Steps.Count - 2].NodeId;

            switch (neighbours.Count)
            {
                case 1:
                    if (neighbours[0].Item1 != previousNodeId) return await JustStep(neighbours[0].Item1, neighbours[0].Item2);
                    if (!isButtonPressed) return false;

                    var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                        { Resources.SID_It_s_an_end_node_, Resources.SID_If_you_need_to_continue__press__Step_backward_ }, -1);
                    await _windowManager.ShowDialogWithAssignedOwner(vm);
                    return false;
                case 2:
                    if (previousNodeId == neighbours[0].Item1)
                    {
                        return await JustStep(neighbours[1].Item1, neighbours[1].Item2);
                    }
                    else if (previousNodeId == neighbours[1].Item1)
                    {
                        return await JustStep(neighbours[0].Item1, neighbours[0].Item2);
                    }
                    else
                        return await ForkIt(neighbours, previousNodeId);
                default:
                    return await ForkIt(neighbours, previousNodeId);
            }
        }

        private async Task<bool> ForkIt(List<Tuple<Guid, List<Guid>>> neighbours, Guid previousNodeId)
        {
            _currentHighlightedNodeVm.IsHighlighted = false;

            if (!await _stepChoiceViewModel.Initialize(neighbours.Select(e => e.Item1).ToList(), previousNodeId))
                return false;
            if (_windowManager.ShowDialogWithAssignedOwner(_stepChoiceViewModel).Result != true)
            {
                await PositionAndHighlightPrevious(_currentHighlightedNodeVm.Id);
                return false;
            }
            var selectedNode = _stepChoiceViewModel.GetSelected();
            var selectedTuple = neighbours.First(n => n.Item1 == selectedNode.NodeId);

            var equipmentId = _graphReadModel.ChooseEquipmentForNode(selectedNode.NodeId, false, out var titleStr);
            if (equipmentId == Guid.Empty)
            {
                await PositionAndHighlightPrevious(_currentHighlightedNodeVm.Id);
                return false;
            }

            Steps.Add(new StepModel() { NodeId = selectedNode.NodeId, Title = titleStr, EquipmentId = equipmentId, FiberIds = selectedTuple.Item2 });
            // _graphReadModel.MainMap.SetPosition(selectedNode.Position);
            _currentHighlightedNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == selectedNode.NodeId);
            _currentHighlightedNodeVm.IsHighlighted = true;
            SetFibersLight(selectedTuple.Item2, true);

            return true;
        }

        private async Task<bool> JustStep(Guid nextNodeId, List<Guid> fiberIdsToNode)
        {
            var nextNode = _readModel.Nodes.First(n => n.NodeId == nextNodeId);

            _currentHighlightedNodeVm.IsHighlighted = false;
            _graphReadModel.MainMap.SetPositionWithoutFiringEvent(nextNode.Position);
            var unused = await _graphReadModel.RefreshVisiblePart();

            var equipmentId = _graphReadModel.ChooseEquipmentForNode(nextNodeId, false, out var titleStr);
            if (equipmentId == Guid.Empty)
            {
                _currentHighlightedNodeVm.IsHighlighted = true;
                _graphReadModel.MainMap.SetPositionWithoutFiringEvent(_currentHighlightedNodeVm.Position);
                var unused2 = await _graphReadModel.RefreshVisiblePart();
                return false;
            }

            Steps.Add(new StepModel() { NodeId = nextNodeId, Title = titleStr, EquipmentId = equipmentId, FiberIds = fiberIdsToNode });
            _currentHighlightedNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == nextNodeId);
            _currentHighlightedNodeVm.IsHighlighted = true;
            SetFibersLight(fiberIdsToNode, true);

            return true;
        }

        private async Task PositionAndHighlightPrevious(Guid previousNodeId)
        {
            _currentHighlightedNodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == previousNodeId);
            if (_currentHighlightedNodeVm == null)
            {
                var node = _readModel.Nodes.First(n => n.NodeId == previousNodeId);
                _graphReadModel.MainMap.SetPositionWithoutFiringEvent(node.Position);
                var _ = await _graphReadModel.RefreshVisiblePart();
                _currentHighlightedNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == previousNodeId);
            }
            else
                _graphReadModel.MainMap.SetPosition(_currentHighlightedNodeVm.Position);
            _currentHighlightedNodeVm.IsHighlighted = true;
        }

        public async void CancelStep()
        {
            if (Steps.Count == 1) return;
            _currentHighlightedNodeVm.IsHighlighted = false;

            SetFibersLight(Steps.Last().FiberIds, false);
            Steps.Remove(Steps.Last());

            await PositionAndHighlightPrevious(Steps.Last().NodeId);
        }

        public async void Accept()
        {
            IsButtonsEnabled = false;
            var result = await AcceptProcedure();
            IsButtonsEnabled = true;
            if (result)
                await TryCloseAsync();
        }

        private async Task<bool> AcceptProcedure()
        {
            if (!Validate()) return false;

            GetListsAugmentedWithAdjustmentPoints(out var traceNodes, out var traceEquipments);
            var traceAddViewModel = _globalScope.Resolve<TraceInfoViewModel>();
            await traceAddViewModel.Initialize(_newTraceId, traceEquipments, traceNodes, true);
            _windowManager.ShowDialogWithAssignedOwner(traceAddViewModel);

            if (!traceAddViewModel.IsSavePressed) return false;

            _currentHighlightedNodeVm.IsHighlighted = false;
            return true;
        }

        private void GetListsAugmentedWithAdjustmentPoints(out List<Guid> nodes, out List<Guid> equipments)
        {
            nodes = new List<Guid> { Steps.First().NodeId };
            equipments = new List<Guid>() { Steps.First().EquipmentId };

            for (int i = 1; i < Steps.Count; i++)
            {
                if (_readModel.Fibers.FirstOrDefault(f =>
                        f.NodeId1 == Steps[i - 1].NodeId && f.NodeId2 == Steps[i].NodeId ||
                        f.NodeId2 == Steps[i - 1].NodeId && f.NodeId1 == Steps[i].NodeId) == null)
                {
                    _readModel.FindPathWhereAdjustmentPointsOnly(Steps[i - 1].NodeId, Steps[i].NodeId, out var pathNodeIds);
                    foreach (var nodeId in pathNodeIds)
                    {
                        nodes.Add(nodeId);
                        equipments.Add(_readModel.Equipments.First(e => e.NodeId == nodeId).EquipmentId);
                    }
                }

                nodes.Add(Steps[i].NodeId);
                equipments.Add(Steps[i].EquipmentId);
            }
        }

        public void AddNodeIntoFiber(NodeIntoFiberAdded evnt)
        {
            StepModel step;
            while ((step = Steps.FirstOrDefault(s => s.FiberIds.Contains(evnt.FiberId))) != null)
            {
                var pos = Steps.IndexOf(step);

                if (evnt.InjectionType == EquipmentType.AdjustmentPoint)
                {
                    var neighbours = _readModel.GetNeighboursPassingThroughAdjustmentPoints(Steps[pos - 1].NodeId);
                    var tuple = neighbours.First(t => t.Item1 == step.NodeId);
                    step.FiberIds = tuple.Item2;
                }
                else
                {
                    var newStep1 = new StepModel()
                    {
                        NodeId = evnt.Id,
                        Title = "",
                        EquipmentId = evnt.EquipmentId,
                    };
                    var neighbours = _readModel.GetNeighboursPassingThroughAdjustmentPoints(Steps[pos - 1].NodeId);
                    var tuple = neighbours.First(t => t.Item1 == evnt.Id);
                    newStep1.FiberIds = tuple.Item2;

                    var newStep2 = new StepModel()
                    {
                        NodeId = step.NodeId,
                        Title = step.Title,
                        EquipmentId = step.EquipmentId,
                    };
                    var neighbours2 = _readModel.GetNeighboursPassingThroughAdjustmentPoints(evnt.Id);
                    var tuple2 = neighbours2.First(t => t.Item1 == step.NodeId);
                    newStep2.FiberIds = tuple2.Item2;

                    Steps.Remove(step);
                    Steps.Insert(pos, newStep1);
                    Steps.Insert(pos + 1, newStep2);
                }
            }
        }

        public void UpdateNode(Guid nodeId)
        {
            for (int i = 1; i < Steps.Count; i++)
            {
                if (Steps[i].NodeId == nodeId)
                {
                    var step = Steps[i];
                    var nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == nodeId);
                    step.Title = nodeVm.Title;

                    var equipment = _readModel.Equipments.FirstOrDefault(e => e.EquipmentId == step.EquipmentId);
                    if (equipment == null)
                    {
                        equipment = _readModel.Equipments.First(e => e.NodeId == nodeId);
                        step.EquipmentId = equipment.EquipmentId;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(equipment.Title))
                            step.Title += @" / " + equipment.Title;
                    }

                    Steps.Remove(step);
                    Steps.Insert(i, step);
                }
            }
        }

        public bool IsNodeUsed(Guid nodeId)
        {
            return Steps.Any(s => s.NodeId == nodeId);
        }

        private bool Validate()
        {
            if (Steps.Count <= 1) return false;
            var equipment = _readModel.Equipments.First(e => e.EquipmentId == Steps.Last().EquipmentId);
            if (equipment.Type <= EquipmentType.EmptyNode)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Last_node_of_trace_must_contain_some_equipment));
                return false;
            }
            return true;
        }

        public async void Cancel()
        {
            _currentHighlightedNodeVm.IsHighlighted = false;
            foreach (var fiberIds in Steps.Select(s => s.FiberIds))
            {
                SetFibersLight(fiberIds, false);
            }
            await TryCloseAsync();
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            IsOpen = false;
            _graphReadModel.MainMap.IsInTraceDefinitionMode = false;
            _commonStatusBarViewModel.StatusBarMessage2 = "";
            return true;
        }

        private void SetFibersLight(IEnumerable<Guid> fiberIds, bool light)
        {
            foreach (var fiberId in fiberIds)
            {
                var fiber = _readModel.Fibers.First(f => f.FiberId == fiberId);
                fiber.SetLightOnOff(_newTraceId, light);

                var fiberVm = _graphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                if (fiberVm != null)
                    fiberVm.SetLightOnOff(_newTraceId, light);
            }
        }
    }
}
