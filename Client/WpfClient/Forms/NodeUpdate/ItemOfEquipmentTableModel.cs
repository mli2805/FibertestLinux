using System;
using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class ItemOfEquipmentTableModel : PropertyChangedBase
    {
        public Guid Id { get; set; }

        public Guid NodeId { get; set; }

        private string _type = null!;
        public string Type
        {
            get => _type;
            set
            {
                if (value == _type) return;
                _type = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _title;
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

        private string _cableReserveLeft = null!;
        public string CableReserveLeft
        {
            get => _cableReserveLeft;
            set
            {
                if (value == _cableReserveLeft) return;
                _cableReserveLeft = value;
                NotifyOfPropertyChange();
            }
        }

        private string _cableReserveRight = null!;
        public string CableReserveRight
        {
            get => _cableReserveRight;
            set
            {
                if (value == _cableReserveRight) return;
                _cableReserveRight = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _comment;
        public string? Comment
        {
            get => _comment;
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        private string _traces = null!;
        public string Traces
        {
            get => _traces;
            set
            {
                if (value == _traces) return;
                _traces = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isRemoveEnabled;
        public bool IsRemoveEnabled
        {
            get => _isRemoveEnabled;
            set
            {
                if (value == _isRemoveEnabled) return;
                _isRemoveEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        private object _command = null!;
        public object Command
        {
            get => _command;
            set
            {
                if (Equals(value, _command)) return;
                _command = value;
                NotifyOfPropertyChange();
            }
        }

        // context menu
        public void UpdateEquipment()
        {
            Command = new UpdateEquipment { EquipmentId = Id };
        }

        public void RemoveEquipment()
        {
            Command = new RemoveEquipment { EquipmentId = Id, NodeId = NodeId };
        }

        public void AddEquipment() { Command = false; }
        public void AddCableReserve() { Command = true; }
    }
}