using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class ConfigurationViewModel : Screen
    {
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly CurrentClientConfiguration _currentClientConfiguration;
        private readonly SoundManager _soundManager;
        public List<string> SupportedLanguages { get; set; } = new List<string>() { @"ru-RU", @"en-US" };

        private bool _isSoundOn;
        private string _soundButtonContent;
        public string SoundButtonContent
        {
            get => _soundButtonContent;
            set
            {
                if (value == _soundButtonContent) return;
                _soundButtonContent = value;
                NotifyOfPropertyChange();
            }
        }

        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                _config.Update(c => c.General.Culture = _selectedLanguage);
            }
        }

        private bool _doNotSignalAboutSuspicion;
        public bool DoNotSignalAboutSuspicion
        {
            get => _doNotSignalAboutSuspicion;
            set
            {
                if (value == _doNotSignalAboutSuspicion) return;
                _doNotSignalAboutSuspicion = value;
                NotifyOfPropertyChange();
                _config.Update(c => c.Miscellaneous.DoNotSignalAboutSuspicion = _doNotSignalAboutSuspicion);
                _currentClientConfiguration.DoNotSignalAboutSuspicion = _doNotSignalAboutSuspicion;
            }
        }

        public ConfigurationViewModel(IWritableConfig<ClientConfig> config, 
            CurrentClientConfiguration currentClientConfiguration, SoundManager soundManager)
        {
            _config = config;
            _currentClientConfiguration = currentClientConfiguration;
            _soundManager = soundManager;

            _selectedLanguage = _config.Value.General.Culture;
            DoNotSignalAboutSuspicion = _config.Value.Miscellaneous.DoNotSignalAboutSuspicion;
            _soundButtonContent = Resources.SID_Turn_alarm_on;
            _isSoundOn = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Client_settings;
        }

        public void TestSound()
        {
            if (_isSoundOn) _soundManager.StopAlert(); else _soundManager.StartAlert();
            _isSoundOn = !_isSoundOn;
            SoundButtonContent = _isSoundOn ? Resources.SID_Turn_alarm_off : Resources.SID_Turn_alarm_on;
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (_isSoundOn)
                _soundManager.StopAlert();
            await Task.Delay(0, cancellationToken);
            return true;
        }

        public async void Close()
        {
            await TryCloseAsync();
        }
    }
}
