using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class ObjectToZonesModel : PropertyChangedBase
    {
        public string SubjectTitle { get; set; }
        public Guid RtuId { get; set; }
        public Guid TraceId { get; set; }

        private bool _isRtu;

        public bool IsRtu
        {
            get => _isRtu;
            set
            {
                if (value == _isRtu) return;
                _isRtu = value;
                NotifyOfPropertyChange();
            }
        }

        private List<BoolWithNotification> _isInZones = new List<BoolWithNotification>();
        public List<BoolWithNotification> IsInZones
        {
            get => _isInZones;
            set
            {
                if (Equals(value, _isInZones)) return;
                _isInZones = value;
                NotifyOfPropertyChange();
            }
        }

    }
}