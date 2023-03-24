using System.Diagnostics;
using Autofac;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public partial class MainMenuViewModel
    {
        public async void LaunchComponentsReport()
        {
            _componentsReportViewModel.Initialize();
           await _windowManager.ShowDialogWithAssignedOwner(_componentsReportViewModel);
            if (_componentsReportViewModel.HtmlReport == null) return;

            var pdfFileName = 
                _componentsReportViewModel.HtmlReport
                    .SaveHtmlAsPdf("MonitoringSystemComponentsReport");
            Process.Start(new ProcessStartInfo() { FileName = pdfFileName, UseShellExecute = true });
        }

        public async void LaunchOpticalEventsReport()
        {
            _opticalEventsReportViewModel.Initialize();
            await _windowManager.ShowDialogWithAssignedOwner(_opticalEventsReportViewModel);
            var htmlReport = _opticalEventsReportViewModel.HtmlReport;
            if (htmlReport == null) return;

            var htmlFileName = htmlReport.SaveHtml("OpticalEventsReport");
            Process.Start(new ProcessStartInfo() { FileName = htmlFileName, UseShellExecute = true });


            // var pdfFileName = 
            //     htmlReport.SaveHtmlAsPdf("OpticalEventsReport");
            // Process.Start(new ProcessStartInfo() { FileName = pdfFileName, UseShellExecute = true });
        }

        public async void LaunchEventLogView()
        {
            var vm = _globalScope.Resolve<EventLogViewModel>();
            vm.Initialize();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}
