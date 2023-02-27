using System;
using System.Windows.Media;
using System.Windows.Threading;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfCommonViews
{
    public class SoundManager
    {
        private readonly ILogger _logger;
        private MediaPlayer _alertPlayer = null!;
        private DispatcherTimer _alertTimer = null!;

        private MediaPlayer _okPlayer = null!;

        private int _alertCounter;

        public SoundManager(ILogger logger)
        {
            _logger = logger;
            InitializeAlertPlayer();
            InitializeOkPlayer();
        }

        private void InitializeAlertPlayer()
        {
            _alertPlayer = new MediaPlayer();
            var alertUri = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"Resources\Sounds\Accident.mp3");
            _alertPlayer.Open(alertUri);
            _alertTimer = new DispatcherTimer(TimeSpan.FromSeconds(14.2),
                DispatcherPriority.Background, (_, _) => PlayAlert(), Dispatcher.CurrentDispatcher);
            _alertTimer.IsEnabled = false;
            _alertCounter = 0;
        }

        private void InitializeOkPlayer()
        {
            _okPlayer = new MediaPlayer();
            var alertUri = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\Resources\Sounds\Ok.mp3");
            _okPlayer.Open(alertUri);
        }

        public void StartAlert()
        {
            _alertCounter++;
            if (_alertCounter == 1)
            {
                PlayAlert();
                _alertTimer.IsEnabled = true;
            }
        }

        public void StopAlert()
        {
            _alertCounter--;
            if (_alertCounter == 0)
            {
                _alertTimer.IsEnabled = false;
                _alertPlayer.Stop();
            }
        }

        private void PlayAlert()
        {
            _alertPlayer.Position = TimeSpan.Zero;
            _alertPlayer.Play();
        }

        public void PlayOk()
        {
            _logger.Info(Logs.Client, "PlayOk invocation");
            _okPlayer.Position = TimeSpan.Zero;
            _okPlayer.Play();
        }
    }
}