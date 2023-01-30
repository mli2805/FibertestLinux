using System;
using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class RadioButtonModel : PropertyChangedBase
    {
        public string Title { get; set; }

        public Guid Id { get; set; }

        public bool IsEnabled { get; set; } = true;

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

    }
}