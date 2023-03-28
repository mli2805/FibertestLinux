using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.WpfClient
{
    public class CurrentGis : PropertyChangedBase
    {
        private readonly IWritableConfig<ClientConfig> _config;
        private GpsInputMode _gpsInputMode;

        public GpsInputMode GpsInputMode
        {
            get => _gpsInputMode;
            set
            {
                if (value == _gpsInputMode) return;
                _gpsInputMode = value;
                _config.Update(c=>c.Miscellaneous.GpsInputMode = _gpsInputMode);
                NotifyOfPropertyChange(nameof(GpsInputMode));
            }
        }

        private bool _isWithoutMapMode;
        public bool IsWithoutMapMode
        {
            get { return _isWithoutMapMode; }
            set
            {
                if (value == _isWithoutMapMode) return;
                _isWithoutMapMode = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsGisOn));
            }
        }

        private bool _isRootTempGisOn;
        public bool IsRootTempGisOn
        {
            get { return _isRootTempGisOn; }
            set
            {
                if (value == _isRootTempGisOn) return;
                _isRootTempGisOn = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsGisOn));
            }
        }

        public bool IsGisOn => !IsWithoutMapMode || IsRootTempGisOn;

        public CurrentGis(IWritableConfig<ClientConfig> config)
        {
            _config = config;
            _gpsInputMode = _config.Value.Miscellaneous.GpsInputMode;
            IsHighDensityGraph = _config.Value.Map.IsHighDensityGraph;
            ThresholdZoom = _config.Value.Map.ThresholdZoom;
        }

        private bool _isHighDensityGraph;
        public bool IsHighDensityGraph  
        {
            get => _isHighDensityGraph;
            set
            {
                if (value == _isHighDensityGraph) return;
                _isHighDensityGraph = value;
                NotifyOfPropertyChange();
            }
        }

        public int ThresholdZoom { get; set; }
        public double ScreenPartAsMargin { get; set; }
    }

}