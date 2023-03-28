using System;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Fibertest.WpfClient
{
    public class LandmarkRow : PropertyChangedBase
    {
        public Guid NodeId { get; set; }
        public Guid FiberId { get; set; } // to the left
        public int Number { get; set; }
        public int NumberIncludingAdjustmentPoints { get; set; }

        private string _nodeTitle = "";
        public string NodeTitle
        {
            get => _nodeTitle;
            set
            {
                if (value == _nodeTitle) return;
                _nodeTitle = value;
                NotifyOfPropertyChange();
            }
        }

        private string _nodeComment = "";
        public string NodeComment
        {
            get => _nodeComment;
            set
            {
                if (value == _nodeComment) return;
                _nodeComment = value;
                NotifyOfPropertyChange();
            }
        }

        public Guid EquipmentId { get; set; }
        public string EquipmentTitle { get; set; } = "";
        public string EquipmentType { get; set; } = "";
        public string CableReserves { get; set; } = "";
        public string GpsDistance { get; set; } = ""; // by GPS, ignore cable reserve
        public string GpsSection { get; set; } = "";
        public bool IsUserInput { get; set; }
        public Brush GpsSectionBrush => IsUserInput ? Brushes.LightGray : Brushes.Transparent;
        public FontStyle GpsSectionFontStyle => IsUserInput ? FontStyles.Italic : FontStyles.Normal;
        public string OpticalDistance { get; set; } = ""; // from sor
        public string OpticalSection { get; set; } = "";
        public string EventNumber { get; set; } = "";

        private string _gpsCoors = "";
        public string GpsCoors
        {
            get => _gpsCoors;
            set
            {
                if (value == _gpsCoors) return;
                _gpsCoors = value;
                NotifyOfPropertyChange();
            }
        }

        public LandmarkRow FromLandmark(Landmark landmark, GpsInputMode mode)
        {
            Number = landmark.Number;
            NumberIncludingAdjustmentPoints = landmark.NumberIncludingAdjustmentPoints;
            NodeId = landmark.NodeId;
            FiberId = landmark.FiberId;
            NodeTitle = landmark.NodeTitle ?? "";
            NodeComment = landmark.NodeComment ?? "";
            EquipmentId = landmark.EquipmentId;
            EquipmentTitle = landmark.EquipmentTitle ?? "";
            EquipmentType = landmark.EquipmentType.ToLocalizedString();
            CableReserves = CableReserveToString(landmark);
            GpsDistance = $@"{landmark.GpsDistance: 0.000}";
            GpsSection = landmark.EquipmentType == Dto.EquipmentType.Rtu ? "" : $@"{landmark.GpsSection: 0.000}";
            IsUserInput = landmark.IsUserInput;
            OpticalDistance = landmark.IsFromBase ? $@"{landmark.OpticalDistance: 0.000}" : "";
            OpticalSection = landmark.EquipmentType == Dto.EquipmentType.Rtu ? "" 
                : landmark.IsFromBase ? $@"{landmark.OpticalSection: 0.000}" : "";
            EventNumber = landmark.EventNumber == -1 ? Resources.SID_no : $@"{landmark.EventNumber}";
            GpsCoors = landmark.GpsCoors.ToDetailedString(mode);

            return this;
        }

        private string CableReserveToString(Landmark landmark)
        {
            if (landmark.EquipmentType == Dto.EquipmentType.CableReserve)
            {
                return $@"{landmark.LeftCableReserve}";
            } 

            if (landmark.EquipmentType > Dto.EquipmentType.CableReserve &&
                     landmark.EquipmentType < Dto.EquipmentType.RtuAndEot)
            {
                return $@"{landmark.LeftCableReserve} / {landmark.RightCableReserve}";
            }

            return "";
        }
    }
}