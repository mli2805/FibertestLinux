using System;
using System.Threading;
using Fibertest.Dto;
using Fibertest.Utils;
using GrpsClientLib;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class Heartbeater
    {
        private readonly ILogger _logger;
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly CurrentUser _currentUser;
        private readonly int _heartbeatRate;
        private CancellationToken _token;

        public Heartbeater(IWritableConfig<ClientConfig> config, ILogger logger, GrpcC2DRequests grpcC2DRequests,
             CurrentUser currentUser)
        {
            _logger = logger;
            _grpcC2DRequests = grpcC2DRequests;
            _currentUser = currentUser;
            _heartbeatRate = config.Value.General.ClientHeartbeatRateMs;
        }

        public void Start(CancellationToken token)
        {
            _token = token;
            _logger.LogInfo(Logs.Client, @"Heartbeats started");
            var heartbeaterThread = new Thread(SendHeartbeats) { IsBackground = true };
            heartbeaterThread.Start();
        }

        private async void SendHeartbeats()
        {
            while (!_token.IsCancellationRequested)
            {
                var dto = new ClientHeartbeatDto { ClientConnectionId = _currentUser.ConnectionId };

                var ra = await _grpcC2DRequests.SendAnyC2DRequest<ClientHeartbeatDto, RequestAnswer>(dto);
                if (ra.ReturnCode != ReturnCode.Ok)
                    _logger.LogError(Logs.Client, $"Failed to send heartbeat! {ra.ErrorMessage}");
                Thread.Sleep(TimeSpan.FromMilliseconds(_heartbeatRate));
            }
            _logger.LogInfo(Logs.Client, @"Leaving Heartbeats...");
        }
    }
}
