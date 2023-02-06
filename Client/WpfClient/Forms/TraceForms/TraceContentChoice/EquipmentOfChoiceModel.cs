using System;
using System.Windows;
using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class EquipmentOfChoiceModel : PropertyChangedBase
    {
        public Guid EquipmentId;

        private bool _isSelected;
        private int _leftCableReserve;
        private int _rightCableReserve;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _titleOfEquipment;
        public string? TitleOfEquipment  
        {
            get => _titleOfEquipment;
            set
            {
                if (value == _titleOfEquipment) return;
                _titleOfEquipment = value;
                NotifyOfPropertyChange();
            }
        }

        public string TypeOfEquipment { get; set; }

        public int LeftCableReserve
        {
            get => _leftCableReserve;
            set
            {
                if (value == _leftCableReserve) return;
                _leftCableReserve = value;
                NotifyOfPropertyChange();
            }
        }

        public int RightCableReserve
        {
            get => _rightCableReserve;
            set
            {
                if (value == _rightCableReserve) return;
                _rightCableReserve = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsRadioButtonEnabled { get; set; }

        public Visibility RightCableReserveVisible { get; set; }
    }
}
