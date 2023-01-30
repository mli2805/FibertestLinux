using System;
using System.Threading;
using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class Heartbeater
    {
        private readonly ILogger<Heartbeater> _logger; 
        private readonly IWcfServiceDesktopC2D _wcfConnection;
        private readonly CurrentUser _currentUser;
        private readonly int _heartbeatRate;
        private Thread _heartbeaterThread;
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public Heartbeater(IWritableConfig<ClientConfig> config, ILogger<Heartbeater> logger, 
            IWcfServiceDesktopC2D wcfConnection, CurrentUser currentUser)
        {
            _logger = logger;
            _wcfConnection = wcfConnection;
            _currentUser = currentUser;
            _heartbeatRate = config.Value.General.ClientHeartbeatRateMs;
        }

        public void Start()
        {
            _logger.LogInfo(Logs.Client,@"Heartbeats started");
            _heartbeaterThread = new Thread(SendHeartbeats) { IsBackground = true };
            _heartbeaterThread.Start();
        }

        private async void SendHeartbeats()
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                await _wcfConnection.SendHeartbeat(new HeartbeatDto(){ConnectionId = _currentUser.ConnectionId});
                Thread.Sleep(TimeSpan.FromMilliseconds(_heartbeatRate));
            }
            _logger.LogInfo(Logs.Client,@"Leaving Heartbeats...");
        }
    }
}
