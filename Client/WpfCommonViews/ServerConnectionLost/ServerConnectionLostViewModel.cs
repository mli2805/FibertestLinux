using System;
using Caliburn.Micro;

namespace Fibertest.WpfCommonViews
{
    public class ServerConnectionLostViewModel : Screen
    {
        private readonly SoundManager _soundManager;
        public string ServerLine { get; set; } = null!;
        public string Timestamp { get; set; } = null!;

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (value == _isOpen) return;
                _isOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public ServerConnectionLostViewModel(SoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        public void Initialize(string serverTitle, string serverIp)
        {
            ServerLine = $@"{serverTitle} ({serverIp})";
            Timestamp = $"{DateTime.Now:G}";
            IsOpen = true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
            _soundManager.StartAlert();
        }

        public void TurnSoundOff()
        {
            _soundManager.StopAlert();
        }

        public void CloseApplication()
        {
            IsOpen = false;
        }
    }
}
