using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class TraceStateModel : PropertyChangedBase
    {
        public Guid TraceId { get; set; }
        public Trace Trace { get; set; }
        public TraceStateModelHeader Header { get; set; }
        public FiberState TraceState { get; set; }
        public BaseRefType BaseRefType { get; set; }

        public Brush TraceStateBrush =>
            TraceState == FiberState.Ok
                ? Brushes.White
                : BaseRefType == BaseRefType.Fast
                    ? Brushes.Yellow
                    : TraceState.GetBrush(isForeground: true);

        private string PrepareTraceStateOnScreen ()
        {
            if (TraceState == FiberState.Ok) return @"OK";
            if (BaseRefType == BaseRefType.Fast)
            {
                return TraceState != FiberState.NoFiber
                    ? FiberState.Suspicion.ToLocalizedString()
                    : $@"{FiberState.Suspicion.ToLocalizedString()}  ({FiberState.NoFiber.ToLocalizedString()})";
            }
            return TraceState.ToLocalizedString();
        }

        public string TraceStateOnScreen => PrepareTraceStateOnScreen();

        public EventStatus EventStatus { get; set; } = EventStatus.EventButNotAnAccident;
        public string Comment { get; set; }

        public DateTime MeasurementTimestamp { get; set; }
        public DateTime RegistrationTimestamp { get; set; }
        public int SorFileId { get; set; }
        public string StateAt => string.Format(Resources.SID_State_at_, MeasurementTimestamp.ToString(CultureInfo.CurrentCulture), SorFileId);

        public Visibility OpticalEventPanelVisibility
            => EventStatus > EventStatus.EventButNotAnAccident ? Visibility.Visible : Visibility.Collapsed;

        public List<AccidentLineModel> Accidents { get; set; } = new List<AccidentLineModel>();
        public AccidentLineModel? SelectedAccident { get; set; }

       
        public Visibility AccidentsPanelVisibility
            => TraceState == FiberState.Ok || TraceState == FiberState.NoFiber ? Visibility.Collapsed : Visibility.Visible;

        public string AccidentsHeader => string.Format(Resources.SID_Accidents_count___0_, Accidents.Count);

        public bool IsLastStateForThisTrace { get; set; }
        public bool IsLastAccidentForThisTrace { get; set; }
        public bool IsAccidentPlaceButtonEnabled => TraceState != FiberState.Ok && IsLastAccidentForThisTrace;

        private bool _isSoundButtonEnabled;

        public bool IsSoundButtonEnabled
        {
            get => _isSoundButtonEnabled;
            set
            {
                if (value == _isSoundButtonEnabled) return;
                _isSoundButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
