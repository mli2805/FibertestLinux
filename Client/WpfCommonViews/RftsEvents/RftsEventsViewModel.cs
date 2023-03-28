using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.OtdrDataFormat;
using Fibertest.StringResources;
using Fibertest.Utils;

namespace Fibertest.WpfCommonViews
{
    public class RftsEventsViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private bool _isNoFiber;
        private string _traceTitle = null!;
        public Visibility NoFiberLabelVisibility => _isNoFiber ? Visibility.Visible : Visibility.Collapsed;
        public Visibility RftsEventsTableVisibility => _isNoFiber ? Visibility.Collapsed : Visibility.Visible;

        public LevelsContent LevelsContent { get; set; } = new LevelsContent();
        public RftsEventsFooterViewModel FooterViewModel { get; set; } = null!;

        public int SelectedIndex { get; set; } = -1;

        public RftsEventsViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void Initialize(OtdrDataKnownBlocks sorData, string traceTitle = "")
        {
            _traceTitle = traceTitle;
            _isNoFiber = sorData.RftsEvents.MonitoringResult == (int) ComparisonReturns.NoFiber;
            if (_isNoFiber) return;

            var rftsEventsBlocks = sorData.GetRftsEventsBlockForEveryLevel().ToList();

            var rftsParameters = sorData.RftsParameters;
            for (int i = 0; i < rftsParameters.LevelsCount; i++)
            {
                var level = rftsParameters.Levels[i];
                if (level.IsEnabled)
                    switch (level.LevelName)
                    {
                        case RftsLevelType.Minor:
                            LevelsContent.IsMinorExists = true;
                            LevelsContent.MinorLevelViewModel = 
                                new RftsEventsOneLevelViewModel(sorData, rftsEventsBlocks.FirstOrDefault(b=>b.LevelName == level.LevelName), level);
                            if (SelectedIndex == -1) SelectedIndex = 0;
                            break;
                        case RftsLevelType.Major:
                            LevelsContent.IsMajorExists = true;
                            LevelsContent.MajorLevelViewModel = 
                                new RftsEventsOneLevelViewModel(sorData, rftsEventsBlocks.FirstOrDefault(b=>b.LevelName == level.LevelName), level);
                            if (SelectedIndex == -1) SelectedIndex = 1;
                            break;
                        case RftsLevelType.Critical:
                            LevelsContent.IsCriticalExists = true;
                            LevelsContent.CriticalLevelViewModel = 
                                new RftsEventsOneLevelViewModel(sorData, rftsEventsBlocks.FirstOrDefault(b=>b.LevelName == level.LevelName), level);
                            if (SelectedIndex == -1) SelectedIndex = 2;
                            break;
                        case RftsLevelType.None:
                            LevelsContent.IsUsersExists = true;
                            LevelsContent.UsersLevelViewModel = 
                                new RftsEventsOneLevelViewModel(sorData, rftsEventsBlocks.FirstOrDefault(b=>b.LevelName == level.LevelName), level);
                            if (SelectedIndex == -1) SelectedIndex = 3;
                            break;
                    }
            }

            FooterViewModel = new RftsEventsFooterViewModel(sorData, LevelsContent);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Rfts_Events + $":  {_traceTitle}";
        }

        public async void ExportToPdf()
        {
            // var report = RftsEventsPdfProvider.Create(LevelsContent.GetAll(), _traceTitle);
            // PdfExposer.Show(report, @"RFTS events.pdf", _windowManager);

            var vm = new MyMessageBoxViewModel(MessageType.Error, "PDF provider not implemented yet");
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void CloseButton()
        {
            await TryCloseAsync();
        }
    }
}
