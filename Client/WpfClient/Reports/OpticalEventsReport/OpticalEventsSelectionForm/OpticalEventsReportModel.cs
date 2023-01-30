using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class OpticalEventsReportModel : PropertyChangedBase
    {
        private bool _isCustomReport;
        public bool IsCustomReport
        {
            get => _isCustomReport;
            set
            {
                if (value == _isCustomReport) return;
                _isCustomReport = value;
                IsCurrentEventsReport = !_isCustomReport;
                NotifyOfPropertyChange();
            }
        }

        public bool IsCurrentEventsReport
        {
            get => _isCurrentEventsReport;
            set
            {
                if (value == _isCurrentEventsReport) return;
                _isCurrentEventsReport = value;
                NotifyOfPropertyChange();
            }
        }

        public DateTime DateFrom
        {
            get => _dateFrom;
            set
            {
                if (value.Equals(_dateFrom)) return;
                _dateFrom = value;
                NotifyOfPropertyChange();
            }
        }

        public DateTime DateTo { get; set; }

        public EventStatusViewModel EventStatusViewModel { get; set; } = new EventStatusViewModel();
        public TraceStateSelectionViewModel TraceStateSelectionViewModel { get; set; } = new TraceStateSelectionViewModel();

        public bool IsZoneSelectionEnabled { get; set; }
        public List<Zone> Zones { get; set; }
        public Zone SelectedZone { get; set; }

        private bool _isDetailedReport = true;

        public bool IsDetailedReport
        {
            get => _isDetailedReport;
            set
            {
                if (value == _isDetailedReport) return;
                _isDetailedReport = value;
                NotifyOfPropertyChange();
                IsAccidentPlaceShown = false;
            }
        }

        private bool _isAccidentPlaceShown;
        private bool _isCurrentEventsReport;
        private DateTime _dateFrom;

        public bool IsAccidentPlaceShown
        {
            get { return _isAccidentPlaceShown; }
            set
            {
                if (value == _isAccidentPlaceShown) return;
                _isAccidentPlaceShown = value;
                NotifyOfPropertyChange();
            }
        }
    }
}