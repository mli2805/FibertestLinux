using System;
using System.Windows.Media;
using Caliburn.Micro;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class MonitoringPortModel : PropertyChangedBase
    {
        private bool _isIncluded;
        public int PortNumber { get; set; }
        public Guid TraceId { get; set; }
        public string TraceTitle { get; set; } = null!;
        public TimeSpan PreciseBaseSpan { get; set; } = TimeSpan.Zero;
        public TimeSpan FastBaseSpan { get; set; } = TimeSpan.Zero;
        // public TimeSpan AdditionalBaseSpan { get; set; } = TimeSpan.Zero;
        public bool IsInCurrentUserZone { get; set; }

        public bool IsIncluded
        {
            get => _isIncluded;
            set
            {
                if (value == _isIncluded) return;
                _isIncluded = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsReadyForMonitoring => PreciseBaseSpan != TimeSpan.Zero && FastBaseSpan != TimeSpan.Zero;

        public bool IsCheckboxEnabled => IsReadyForMonitoring && IsInCurrentUserZone;
        public Brush TraceTitleBrush => IsInCurrentUserZone ? Brushes.Black : Brushes.LightGray;

        public string Duration => FastBaseSpan.TotalSeconds + @" / " + PreciseBaseSpan.TotalSeconds + Resources.SID__sec;
    }
}
