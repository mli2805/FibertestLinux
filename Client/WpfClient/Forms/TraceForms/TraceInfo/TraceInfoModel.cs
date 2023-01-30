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
        public Rtu Rtu;
        public List<Guid> TraceEquipments;
        public List<Guid> TraceNodes;


        public string RtuTitle { get; set; }
        public string PortNumber { get; set; }

        public List<TraceInfoTableItem> NodesRows { get; set; }
        public List<TraceInfoTableItem> EquipmentsRows { get; set; }

        public string AdjustmentPointsLine { get; set; }
        public Visibility AdjustmentPointsLineVisibility { get; set; } = Visibility.Collapsed;

        public string PhysicalLength { get; set; }
        public string OpticalLength { get; set; }
      
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

        private string _comment;
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