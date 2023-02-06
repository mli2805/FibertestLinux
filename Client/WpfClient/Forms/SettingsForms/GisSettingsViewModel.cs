using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using GMap.NET;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class GisSettingsViewModel : Screen
    {
        private readonly CurrentGis _currentGis;
        private readonly DataCenterConfig _currentDatacenterParameters;
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly IWindowManager _windowManager;
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly GraphReadModel _graphReadModel;

        private bool _isInWithoutMapMode;
        public bool IsInWithoutMapMode
        {
            get => _isInWithoutMapMode;
            set
            {
                if (value == _isInWithoutMapMode) return;
                _isInWithoutMapMode = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(GisModeMessage));
            }
        }

        public string GisModeMessage => IsInWithoutMapMode ? Resources.SID_In__Without_Map__mode : Resources.SID_Map_is_displayed;

        public bool IsRoot { get; set; }
        public Visibility SecondBoxVisibility { get; set; }

        private Visibility _thirdBoxVisibility;
        public Visibility ThirdBoxVisibility
        {
            get { return _thirdBoxVisibility; }
            set
            {
                if (value == _thirdBoxVisibility) return;
                _thirdBoxVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public List<string> MapProviders { get; set; } =
            new List<string>() { @"OpenStreetMap", @"GoogleMap", @"GoogleSatelliteMap", @"GoogleHybridMap" };
        private string _selectedProvider;
        public string SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                _selectedProvider = value;
                _config.Update(c => c.Map.GMapProvider = _selectedProvider);
                if (_currentGis.IsGisOn)
                    _graphReadModel.MainMap.MapProvider = GMapProviderExt.Get(_selectedProvider);
            }
        }

        public List<string> AccessModes { get; set; }

        private string _selectedAccessMode;
        public string SelectedAccessMode
        {
            get => _selectedAccessMode;
            set
            {
                if (value == _selectedAccessMode) return;
                _selectedAccessMode = value;

                var mo = AccessModeExt.FromLocalizedString(_selectedAccessMode);
                _config.Update(c => c.Map.MapAccessMode = mo);
                if (_currentGis.IsGisOn)
                    _graphReadModel.MainMap.Manager.Mode = AccessModeExt.FromEnumConstant(_selectedAccessMode);
            }
        }

        private bool _isMapTemporarilyVisibleInThisClient;

        public bool IsMapTemporarilyVisibleInThisClient
        {
            get => _isMapTemporarilyVisibleInThisClient;
            set
            {
                _isMapTemporarilyVisibleInThisClient = value;
                _currentGis.IsRootTempGisOn = value;
                _graphReadModel.MainMap.MapProvider = value
                    ? GMapProviderExt.Get(_config.Value.Map.GMapProvider)
                    : GMapProviders.EmptyProvider;
                ThirdBoxVisibility = _currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public GisSettingsViewModel(CurrentUser currentUser, CurrentGis currentGis,
            DataCenterConfig currentDatacenterParameters,
            GrpcC2DRequests grpcC2DRequests, IWindowManager windowManager,
            IWritableConfig<ClientConfig> config, GraphReadModel graphReadModel)
        {
            _currentGis = currentGis;
            _currentDatacenterParameters = currentDatacenterParameters;
            _grpcC2DRequests = grpcC2DRequests;
            IsInWithoutMapMode = currentGis.IsWithoutMapMode;
            _windowManager = windowManager;

            _config = config;
            _graphReadModel = graphReadModel;

            // AccessModes = Enum.GetValues(typeof(AccessMode))
            //     .Cast<AccessMode>().Select(x => x.ToLocalizedString()).ToList();
            // var str = _config.Value.Map.MapAccessMode;

            // _selectedAccessMode = AccessModeExt.FromEnumConstant(str).ToLocalizedString();
            _selectedAccessMode = _config.Value.Map.MapAccessMode.ToLocalizedString();

            IsRoot = currentUser.Role <= Role.Root;
            SecondBoxVisibility = currentUser.Role <= Role.Root ? Visibility.Visible : Visibility.Collapsed;
            _selectedProvider = _config.Value.Map.GMapProvider;
            ThirdBoxVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Gis_settings;
            _isMapTemporarilyVisibleInThisClient = _currentGis.IsRootTempGisOn;
        }

        public async void ChangeMode()
        {
            RequestAnswer result;
            using (new WaitCursor())
            {
                var dto = new ChangeDcConfigDto() { NewConfig = _currentDatacenterParameters };
                dto.NewConfig.General.IsWithoutMapMode = !IsInWithoutMapMode;

                result = await _grpcC2DRequests.SendAnyC2DRequest<ChangeDcConfigDto, RequestAnswer>(dto);
            }

            if (result.ReturnCode == ReturnCode.Ok)
            {
                _currentDatacenterParameters.General.IsWithoutMapMode = !IsInWithoutMapMode;
                IsInWithoutMapMode = !IsInWithoutMapMode;
                _currentGis.IsWithoutMapMode = IsInWithoutMapMode;
                if (IsInWithoutMapMode && IsMapTemporarilyVisibleInThisClient)
                {
                    var provider = _config.Value.Map.GMapProvider;
                    _graphReadModel.MainMap.MapProvider = GMapProviderExt.Get(provider);
                }
                ThirdBoxVisibility = _currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Failed_to_save_GIS_mode_);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }
    }
}
