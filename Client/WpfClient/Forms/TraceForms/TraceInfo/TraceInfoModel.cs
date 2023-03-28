using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class TraceInfoModel : PropertyChangedBase
    {
        public Guid TraceId { get; set; }
        public Rtu Rtu = null!;
        public List<Guid> TraceEquipments = null!;
        public List<Guid> TraceNodes = null!;


        public string RtuTitle { get; set; } = null!;
        public string PortNumber { get; set; } = null!;

        public List<TraceInfoTableItem> NodesRows { get; set; } = null!;
        public List<TraceInfoTableItem> EquipmentsRows { get; set; } = null!;

        public string AdjustmentPointsLine { get; set; } = null!;
        public Visibility AdjustmentPointsLineVisibility { get; set; } = Visibility.Collapsed;

        public string PhysicalLength { get; set; } = null!;
        public string OpticalLength { get; set; } = null!;
      
        private bool _isTraceModeLight;
        public bool IsTraceModeLight
        {
            get => _isTraceModeLight;
            set
            {
                if (value == _isTraceModeLight) return;
                _isTraceModeLight = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isTraceModeDark;
        public bool IsTraceModeDark
        {
            get => _isTraceModeDark;
            set
            {
                if (value == _isTraceModeDark) return;
                _isTraceModeDark = value;
                NotifyOfPropertyChange();
            }
        }

        private string _comment = string.Empty;
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

   
    }
}