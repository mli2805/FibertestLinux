using System.Windows.Media;
using Fibertest.Dto;
using Fibertest.OtdrDataFormat;
using Fibertest.StringResources;
using Fibertest.Utils;

namespace WpfCommonViews
{
    public class RftsEventsFooterViewModel
    {
        public string TraceState { get; set; } = null!;
        public Brush TraceStateColor => TraceState == Resources.SID_pass ? Brushes.Black : Brushes.Red;
        public double Orl { get; set; }

        public string? Minor { get; set; }
        public Brush MinorColor => Minor == Resources.SID_pass ? Brushes.Black : Brushes.Red;
        public string? Major { get; set; }
        public Brush MajorColor => Major == Resources.SID_pass ? Brushes.Black : Brushes.Red;
        public string? Critical { get; set; }
        public Brush CriticalColor => Critical == Resources.SID_pass ? Brushes.Black : Brushes.Red;
        public string? Users { get; set; }
        public Brush UsersColor => Users == Resources.SID_pass ? Brushes.Black : Brushes.Red;

        public LevelsContent LevelsContent { get; set; }

        public RftsEventsFooterViewModel(OtdrDataKnownBlocks sorData, LevelsContent levelsContent)
        {
            LevelsContent = levelsContent;
            Orl = sorData.KeyEvents.OpticalReturnLoss;

            SetStates(sorData);
        }

        private void SetStates(OtdrDataKnownBlocks sorData)
        {
            TraceState = Resources.SID_pass;
            if (LevelsContent.IsMinorExists)
            {
                Minor = LevelsContent.MinorLevelViewModel!.GetState(); 
                if (LevelsContent.MinorLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Minor;
            }
            if (LevelsContent.IsMajorExists)
            {
                Major = LevelsContent.MajorLevelViewModel!.GetState();
                if (LevelsContent.MajorLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Major;
            }
            if (LevelsContent.IsCriticalExists)
            {
                Critical = LevelsContent.CriticalLevelViewModel!.GetState();
                if (LevelsContent.CriticalLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Critical;
            }
            if (LevelsContent.IsUsersExists)
            {
                Users = LevelsContent.UsersLevelViewModel!.GetState();
                if (LevelsContent.UsersLevelViewModel.IsFailed)
                    TraceState = Resources.SID_User_s;
            }
            if (sorData.RftsEvents.MonitoringResult == (int)ComparisonReturns.FiberBreak)
            {
                var owt = sorData.KeyEvents.KeyEvents[sorData.KeyEvents.KeyEventsCount - 1].EventPropagationTime;
                var breakLocation = sorData.OwtToLenKm(owt);
                TraceState = string.Format(Resources.SID_fiber_break___0_0_00000__km, breakLocation);
            }
        }
    }
}
