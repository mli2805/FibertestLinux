using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class RtuStateModel : PropertyChangedBase
    {
        private FiberState _tracesState;
        public string? Title { get; set; }
        public string? ServerTitle { get; set; }
        public string RtuAvailabilityOnScreen => RtuAvailabilityToString();
        public Brush RtuAvailabilityBrush => RtuAvalilabilityToBrush(true);

        public string? MainAddress { get; set; }
        public RtuPartState MainAddressState { get; set; }
        public string MainAddressStateOnScreen => MainAddressState.ToLocalizedString();
        public Brush MainAddressBrush => MainAddressState.GetBrush(true);

        public bool HasReserveAddress { get; set; }
        public string? ReserveAddress { get; set; }
        public RtuPartState ReserveAddressState { get; set; }
        public string ReserveAddressStateOnScreen => ReserveAddressState.ToLocalizedString();
        public Brush ReserveAddressBrush => ReserveAddressState.GetBrush(true);

        public int FullPortCount { get; set; }
        public int OwnPortCount { get; set; }
        public string PortCountOnScreen => $@"{OwnPortCount}/{FullPortCount}";

        public int TraceCount { get; set; }
        public int BopCount { get; set; }

        public RtuPartState BopState { get; set; }
        public string BopStateOnScreen => BopState.ToLocalizedString();
        public Brush BopStateBrush => BopState.GetBrush(true);

        public FiberState TracesState
        {
            get => _tracesState;
            set
            {
                if (value == _tracesState) return;
                _tracesState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TracesStateOnScreen));
                NotifyOfPropertyChange(nameof(TracesStateBrush));
            }
        }

        public string TracesStateOnScreen => TracesState.ToLocalizedString();
        public Brush TracesStateBrush => TracesState.GetBrush(true);

        private string? _monitoringMode;
        public string? MonitoringMode
        {
            get => _monitoringMode;
            set
            {
                if (value == _monitoringMode) return;
                _monitoringMode = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _currentMeasurementStep;
        public string? CurrentMeasurementStep
        {
            get => _currentMeasurementStep;
            set
            {
                if (value == _currentMeasurementStep) return;
                _currentMeasurementStep = value;
                NotifyOfPropertyChange();
            }
        }

        public List<PortLineModel> Ports { get; set; } = new List<PortLineModel>();

        public void SetWorstTraceStateAsAggregate()
        {
            TracesState = Ports.Count == 0 ? FiberState.Unknown : Ports.Max(p => p.TraceState);
        }

        private bool _isSoundButtonEnabled;
       public bool IsSoundButtonEnabled
        {
            get { return _isSoundButtonEnabled; }
            set
            {
                if (value == _isSoundButtonEnabled) return;
                _isSoundButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        //------------------
        private string RtuAvailabilityToString()
        {
            return MainAddressState == RtuPartState.Ok || ReserveAddressState == RtuPartState.Ok
                ? Resources.SID_Available
                : Resources.SID_Not_available;
        }

        private Brush RtuAvalilabilityToBrush(bool isForeground)
        {
            var state = (int) MainAddressState + ReserveAddressState;
            if (state < 0)
                return Brushes.Red;
            if (state == 0)
                return Brushes.HotPink;
            return isForeground ? Brushes.Black : Brushes.Transparent;
        }
    }
}