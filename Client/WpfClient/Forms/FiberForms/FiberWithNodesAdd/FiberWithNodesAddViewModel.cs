using System.ComponentModel;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class FiberWithNodesAddViewModel : Screen, IDataErrorInfo
    {
        public bool Result { get; set; }

        private string? _count;
        public string? Count
        {
            get { return _count; }
            set
            {
                if (value == _count) return;
                _count = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonSaveEnabled;
        public bool IsButtonSaveEnabled
        {
            get => _isButtonSaveEnabled;
            set
            {
                if (value == _isButtonSaveEnabled) return;
                _isButtonSaveEnabled = value;
                NotifyOfPropertyChange();
            }
        }


        public RadioButtonModel AdjustmentPoint { get; } = new RadioButtonModel { Title = Resources.SID_Adjustment_point, IsChecked = false };
        public RadioButtonModel NodeWithoutEquipment { get; set; } = new RadioButtonModel { Title = Resources.SID_Node_without_equipment, IsChecked = false };
        public RadioButtonModel Closure { get; } = new RadioButtonModel { Title = Resources.SID_Closure, IsChecked = true };
        public RadioButtonModel Cross { get; } = new RadioButtonModel { Title = Resources.SID_Cross, IsChecked = false };
        public RadioButtonModel Terminal { get; } = new RadioButtonModel { Title = Resources.SID_Terminal, IsChecked = false };
        public RadioButtonModel CableReserve { get; set; } = new RadioButtonModel { Title = Resources.SID_CableReserve, IsChecked = false };
        public RadioButtonModel Other { get; } = new RadioButtonModel { Title = Resources.SID_Other, IsChecked = false };

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Section_with_nodes;
        }

        public async void Save()
        {
            Result = true;
            await TryCloseAsync();
        }

        public async void Cancel()
        {
            Result = false;
            await TryCloseAsync();
        }

        public EquipmentType GetSelectedType()
        {
            if (AdjustmentPoint.IsChecked)
                return EquipmentType.AdjustmentPoint;
            if (NodeWithoutEquipment.IsChecked)
                return EquipmentType.EmptyNode;
            if (CableReserve.IsChecked)
                return EquipmentType.CableReserve;

            if (Closure.IsChecked)
                return EquipmentType.Closure;
            if (Cross.IsChecked)
                return EquipmentType.Cross;
            if (Terminal.IsChecked)
                return EquipmentType.Terminal;
            //else if (Other.IsSelected)
            return EquipmentType.Other;
        }

        public void SetSelectedType(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.AdjustmentPoint:
                    AdjustmentPoint.IsChecked = true;
                    break;
                case EquipmentType.EmptyNode:
                    NodeWithoutEquipment.IsChecked = true;
                    break;
                case EquipmentType.CableReserve:
                    CableReserve.IsChecked = true;
                    break;

                case EquipmentType.Closure:
                    Closure.IsChecked = true;
                    break;
                case EquipmentType.Cross:
                    Cross.IsChecked = true;
                    break;
                case EquipmentType.Terminal:
                    Terminal.IsChecked = true;
                    break;
                case EquipmentType.Other:
                    Other.IsChecked = true;
                    break;
            }
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Count":
                        if (!int.TryParse(_count, out int cc) || cc > 60 || cc < 0)
                            errorMessage = Resources.SID_Must_be_number_no_more_than_60;
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; } = string.Empty;
    }
}
