using System;
using Caliburn.Micro;
using Fibertest.Dto;
using GMap.NET;

namespace Fibertest.WpfClient
{
    public class NodeVm : PropertyChangedBase
    {
        public Guid Id { get; set; }

        private string? _title;
        private PointLatLng _position;
        private FiberState _state;
        private EquipmentType _type;
        private bool _isHighlighted;
        private bool _isVisible;
        private GraphVisibilityLevelItem? _graphVisibilityLevelItem;

        public string? Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public EquipmentType Type
        {
            get => _type;
            set
            {
                if (value == _type) return;
                _type = value;
                NotifyOfPropertyChange();
            }
        }

        public FiberState State
        {
            get => _state;
            set
            {
                if (value == _state) return;
                _state = value;
                NotifyOfPropertyChange();
            }
        }

        public PointLatLng Position
        {
            get => _position;
            set
            {
                if (value.Equals(_position)) return;
                _position = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (value == _isHighlighted) return;
                _isHighlighted = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value == _isVisible) return;
                _isVisible = value;
                NotifyOfPropertyChange();
            }
        }


        public GraphVisibilityLevelItem? GraphVisibilityLevelItem
        {
            get { return _graphVisibilityLevelItem; }
            set
            {
                _graphVisibilityLevelItem = value;
            }
        }

        public Guid AccidentOnTraceVmId { get; set; } = Guid.Empty;
    }
}