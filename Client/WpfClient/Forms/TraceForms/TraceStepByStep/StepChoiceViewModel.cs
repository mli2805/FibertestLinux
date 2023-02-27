using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class StepChoiceViewModel : Screen
    {
        private readonly GraphReadModel _graphReadModel;
        private readonly Model _readModel;
        public List<RadioButtonModel> Models { get; set; } = null!;
        private List<Node> _neighbours = null!;
        private Node _selectedNode = null!;

        public StepChoiceViewModel(GraphReadModel graphReadModel, Model readModel)
        {
            _graphReadModel = graphReadModel;
            _readModel = readModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Next_step;
        }

        public async Task<bool> Initialize(List<Guid> neighbours, Guid previousNodeId)
        {
            _neighbours = neighbours.Select(id => _readModel.Nodes.First(n => n.NodeId == id)).ToList();

            Models = new List<RadioButtonModel>();
            foreach (var node in _neighbours)
            {
                var model = new RadioButtonModel()
                {
                    Id = node.NodeId,
                    IsEnabled = true,
                    Title = node.NodeId == previousNodeId 
                        ? node.Title ?? "" + Resources.SID____previous_ : node.Title ?? "",
                };
                model.PropertyChanged += Model_PropertyChanged;
                if (node.NodeId != previousNodeId)
                    Models.Insert(0, model);
                else
                    Models.Add(model); // previous node should be last in Models list
            }

            if (_neighbours.Count == 0) return false;
            _selectedNode = _neighbours.First();
            Models.First().IsChecked = true;

            var nodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _selectedNode.NodeId);
            if (nodeVm == null)
            {
                _graphReadModel.MainMap.SetPositionWithoutFiringEvent(_selectedNode.Position);
                await _graphReadModel.RefreshVisiblePart();
                nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == _selectedNode.NodeId);
            }

            nodeVm.IsHighlighted = true;
            return true;
        }

        private async void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var modelWithChanges = (RadioButtonModel)sender!;

            if (modelWithChanges.IsChecked)
            {
                _selectedNode = _neighbours.First(n => n.NodeId == modelWithChanges.Id);
                _graphReadModel.MainMap.SetPositionWithoutFiringEvent(_selectedNode.Position);
                await _graphReadModel.RefreshVisiblePart();
                var newChoiceNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == _selectedNode.NodeId);
                newChoiceNodeVm.IsHighlighted = true;
            }
            else
            {
                var previousChoiceNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == modelWithChanges.Id);
                previousChoiceNodeVm.IsHighlighted = false;
            }
        }

        public Node GetSelected()
        {
            return _neighbours.First(n => n.NodeId == Models.First(m => m.IsChecked).Id);
        }

        public async void Select()
        {
            _selectedNode.IsHighlighted = false;
            await TryCloseAsync(true);
        }

        public async void Cancel()
        {
            _selectedNode.IsHighlighted = false;
            var  nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == _selectedNode.NodeId);
            nodeVm.IsHighlighted = false;
           await TryCloseAsync(false);
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var radioButtonModel in Models)
            {
                radioButtonModel.PropertyChanged -= Model_PropertyChanged;
            }
            await Task.Delay(0, cancellationToken);
            return true;
        }

    }
}
