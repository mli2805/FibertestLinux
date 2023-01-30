using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class EquipmentInfoModel : PropertyChangedBase
    {
        private string _title;
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

        private int _cableReserveLeft;
        public int CableReserveLeft
        {
            get => _cableReserveLeft;
            set
            {
                if (value == _cableReserveLeft) return;
                _cableReserveLeft = value;
                NotifyOfPropertyChange();
            }
        }

        private int _cableReserveRight;
        public int CableReserveRight
        {
            get => _cableReserveRight;
            set
            {
                if (value == _cableReserveRight) return;
                _cableReserveRight = value;
                NotifyOfPropertyChange();
            }
        }

        private string _comment;
        private bool _isRightCableReserveEnabled;

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

        public bool IsRightCableReserveEnabled
        {
            get => _isRightCableReserveEnabled;
            set
            {
                if (value == _isRightCableReserveEnabled) return;
                _isRightCableReserveEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public RadioButtonModel Sleeve { get; } = new RadioButtonModel() { Title = Resources.SID_Closure };
        public RadioButtonModel Cross { get; } = new RadioButtonModel() { Title = Resources.SID_Cross };
        public RadioButtonModel Terminal { get; } = new RadioButtonModel() { Title = Resources.SID_Terminal };
        public RadioButtonModel CableReserve { get; } = new RadioButtonModel() { Title = Resources.SID_CableReserve };
        public RadioButtonModel Other { get; } = new RadioButtonModel() { Title = Resources.SID_Other };

        // just for mapping
        public EquipmentType Type
        {
            get => GetSelectedRadioButton();
            set => SetSelectedRadioButton(value);
        }

        public EquipmentInfoModel()
        {
            Sleeve.PropertyChanged += RadioButtonPropertyChanged;
            Cross.PropertyChanged += RadioButtonPropertyChanged;
            Terminal.PropertyChanged += RadioButtonPropertyChanged;
            CableReserve.PropertyChanged += RadioButtonPropertyChanged;
            Other.PropertyChanged += RadioButtonPropertyChanged;
        }

        private void RadioButtonPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != @"IsChecked") return;
            var model = (RadioButtonModel) sender;
            if (!model.IsChecked) return;
            IsRightCableReserveEnabled =
                model.Title != Resources.SID_Terminal && model.Title != Resources.SID_CableReserve;
        }

        public EquipmentType GetSelectedRadioButton()
        {
            if (Sleeve.IsChecked)
                return EquipmentType.Closure;
            if (Cross.IsChecked)
                return EquipmentType.Cross;
            if (Terminal.IsChecked)
                return EquipmentType.Terminal;
            if (CableReserve.IsChecked)
                return EquipmentType.CableReserve;
            if (Other.IsChecked)
                return EquipmentType.Other;
            return EquipmentType.Error;
        }

        public void SetSelectedRadioButton(EquipmentType type)
        {
            CleanSelectedRadioButton();
            if (type == EquipmentType.Closure)
                Sleeve.IsChecked = true;
            else if (type == EquipmentType.Cross)
                Cross.IsChecked = true;
            else if (type == EquipmentType.Terminal)
                Terminal.IsChecked = true;
            else if (type == EquipmentType.CableReserve)
                CableReserve.IsChecked = true;
            else if (type == EquipmentType.Other)
                Other.IsChecked = true;
        }

        private void CleanSelectedRadioButton()
        {
            Sleeve.IsChecked = false;
            Cross.IsChecked = false;
            Terminal.IsChecked = false;
            CableReserve.IsChecked = false;
            Other.IsChecked = false;
        }
    }
}