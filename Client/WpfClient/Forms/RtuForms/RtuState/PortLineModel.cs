using System;
using System.Threading;
using System.Windows.Media;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class PortLineModel : PropertyChangedBase
    {
        public string Number { get; set; } = null!;
        public Guid TraceId { get; set; } = Guid.Empty;
        public string? TraceTitle { get; set; }

        private FiberState _traceState;
        public FiberState TraceState
        {
            get => _traceState;
            set
            {
                if (value == _traceState) return;
                _traceState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TraceStateOnScreen));
                NotifyOfPropertyChange(nameof(TraceStateBrush));
            }
        }

        public string TraceStateOnScreen => TraceId == Guid.Empty ? "" : TraceState.ToLocalizedString();
        public Brush TraceStateBrush => TraceState.GetBrush(false);

        private string? _lastSorFileId;
        public string? LastSorFileId
        {
            get => _lastSorFileId;
            set
            {
                if (value == _lastSorFileId) return;
                _lastSorFileId = value;
                NotifyOfPropertyChange();
            }
        }

        private DateTime? _timestamp;
        public DateTime? Timestamp
        {
            get => _timestamp;
            set
            {
                if (value.Equals(_timestamp)) return;
                _timestamp = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TimestampOnScreen));
            }
        }

        public string TimestampOnScreen => Timestamp?.ToString(Thread.CurrentThread.CurrentUICulture) ?? "";
    }
}