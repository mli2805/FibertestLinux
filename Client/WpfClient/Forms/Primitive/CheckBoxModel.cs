using System;
using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class CheckBoxModel : PropertyChangedBase
    {
        private bool _isChecked;
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public bool IsEnabled { get; set; } = true;

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
    }
}