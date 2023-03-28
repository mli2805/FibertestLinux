using System;
using Caliburn.Micro;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public class BaseRefsSet : PropertyChangedBase
    {
        private Guid _preciseId = Guid.Empty;
        private Guid _fastId = Guid.Empty;
        private Guid _additionalId = Guid.Empty;

        public Guid PreciseId
        {
            get => _preciseId;
            set
            {
                if (value.Equals(_preciseId)) return;
                _preciseId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(HasAnyBaseRef));
                NotifyOfPropertyChange(nameof(HasEnoughBaseRefsToPerformMonitoring));
                NotifyOfPropertyChange(nameof(MonitoringPictogram));
            }
        }

        public TimeSpan PreciseDuration { get; set; }

        public Guid FastId
        {
            get => _fastId;
            set
            {
                if (value.Equals(_fastId)) return;
                _fastId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(HasAnyBaseRef));
                NotifyOfPropertyChange(nameof(HasEnoughBaseRefsToPerformMonitoring));
                NotifyOfPropertyChange(nameof(MonitoringPictogram));
            }
        }

        public TimeSpan FastDuration { get; set; }

        public Guid AdditionalId
        {
            get => _additionalId;
            set
            {
                if (value.Equals(_additionalId)) return;
                _additionalId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(HasAnyBaseRef));
            }
        }

        public TimeSpan AdditionalDuration { get; set; }

        public bool HasAnyBaseRef => PreciseId != Guid.Empty || FastId != Guid.Empty || AdditionalId != Guid.Empty;
        public bool HasEnoughBaseRefsToPerformMonitoring => PreciseId != Guid.Empty && FastId != Guid.Empty;

        private MonitoringState _rtuMonitoringState;
        public MonitoringState RtuMonitoringState
        {
            get => _rtuMonitoringState;
            set
            {
                if (value == _rtuMonitoringState) return;
                _rtuMonitoringState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MonitoringPictogram));
            }
        }

        private bool _isInMonitoringCycle;
        public bool IsInMonitoringCycle
        {
            get => _isInMonitoringCycle;
            set
            {
                if (value == _isInMonitoringCycle) return;
                _isInMonitoringCycle = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MonitoringPictogram));
            }
        }

        public string MonitoringPictogram => GetPathToPictogram();

        private string GetPathToPictogram()
        {
            return IsInMonitoringCycle
                ? RtuMonitoringState == MonitoringState.On
                    ? @"pack://application:,,,/Resources/LeftPanel/BlueSquare.png"
                    : @"pack://application:,,,/Resources/LeftPanel/GreySquare.png"
                : HasEnoughBaseRefsToPerformMonitoring
                    ? @"pack://application:,,,/Resources/LeftPanel/GreyHalfSquare.png"
                    : @"pack://application:,,,/Resources/LeftPanel/EmptySquare.png";
        }
    }
}