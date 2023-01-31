using System.Collections.Generic;
using System.Windows.Input;
using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class MenuItemVm : PropertyChangedBase
    {
        public string Header { get; set; } = null!;
        public List<MenuItemVm> Children { get; private set; }
        public ICommand Command { get; set; } = null!;
        public object CommandParameter { get; set; } = null!;

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                NotifyOfPropertyChange();
            }
        }

        public MenuItemVm()
        {
            Children = new List<MenuItemVm>();
        }
    }
}