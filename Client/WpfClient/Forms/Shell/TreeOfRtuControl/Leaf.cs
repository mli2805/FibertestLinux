using System;
using System.Collections.Generic;
using System.Windows.Media;
using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class Leaf : PropertyChangedBase
    {
        public Guid Id { get; set; }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Name));
            }
        }

        private Brush _color = null!;
        public Brush Color
        {
            get => _color;
            set
            {
                if (Equals(value, _color)) return;
                _color = value;
                NotifyOfPropertyChange();
            }
        }

        private Brush _backgroundBrush = null!;
        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            set
            {
                if (Equals(value, _backgroundBrush)) return;
                _backgroundBrush = value;
                NotifyOfPropertyChange();
            }
        }

        public virtual string Name { get; set; } = "";

        public List<MenuItemVm> MyContextMenu => GetMenuItems();

        protected virtual List<MenuItemVm> GetMenuItems() { return new List<MenuItemVm>(); }

        public Leaf Parent { get; set; } = null!;

        private bool _isSelected;
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

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value == _isExpanded) return;
                _isExpanded = value;
                NotifyOfPropertyChange();
            }
        }
    }
}