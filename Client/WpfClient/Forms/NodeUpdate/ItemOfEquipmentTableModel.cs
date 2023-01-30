using System;
using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class ItemOfEquipmentTableModel : PropertyChangedBase
    {
        private string _title;
        private string _type;
        private string _comment;
        private string _traces;
        private bool _isRemoveEnabled;
        private object _command;
        private string _cableReserveLeft;
        private string _cableReserveRight;

        public Guid Id { get; set; }

        public Guid NodeId { get; set; }
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

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

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

        public string Comment
        {
            get => _comment;
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

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