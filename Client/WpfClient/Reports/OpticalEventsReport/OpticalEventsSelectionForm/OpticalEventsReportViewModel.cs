using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class OpticalEventsReportViewModel : Screen
    {
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly ActualOpticalEventsReportProvider _actualOpticalEventsReportProvider;
        private readonly AllOpticalEventsReportProvider _allOpticalEventsReportProvider;

        public OpticalEventsReportModel Model { get; set; } = new OpticalEventsReportModel();
        public string? HtmlReport { get; set; }

        public OpticalEventsReportViewModel(CurrentUser currentUser, Model readModel,
            ActualOpticalEventsReportProvider actualOpticalEventsReportProvider, 
            AllOpticalEventsReportProvider allOpticalEventsReportProvider)
        {
            _currentUser = currentUser;
            _readModel = readModel;
            _actualOpticalEventsReportProvider = actualOpticalEventsReportProvider;
            _allOpticalEventsReportProvider = allOpticalEventsReportProvider;

            Model.IsCustomReport = true;

            Model.DateTo = DateTime.Now;
            Model.DateFrom = new DateTime(DateTime.Today.Year, 1, 1);
        }

        public void Initialize()
        {
            Model.IsZoneSelectionEnabled = _currentUser.ZoneId == Guid.Empty;
            Model.Zones = Model.IsZoneSelectionEnabled 
                ? _readModel.Zones 
                : new List<Zone>() {_readModel.Zones.First(z => z.ZoneId == _currentUser.ZoneId)};
            Model.SelectedZone = Model.Zones.First();

            // Model.DateTo = DateTime.Now;
            // Model.DateFrom = new DateTime(DateTime.Today.Year, 1, 1);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Optical_events_report;
            HtmlReport = null;
        }

        public async void CreateReport()
        {
            HtmlReport =  Model.IsCustomReport 
                ? _allOpticalEventsReportProvider.Create(Model) 
                : _actualOpticalEventsReportProvider.Create(Model);
            await TryCloseAsync();
        }
    }
}
