using System;
using System.Threading;
using Fibertest.Dto;
using Fibertest.GrpcClientLib;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class Heartbeater
    {
        private readonly ILogger _logger;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly CurrentUser _currentUser;
        private readonly int _heartbeatRate;
        private CancellationToken _token;

        public Heartbeater(IWritableConfig<ClientConfig> config, ILogger logger, GrpcC2DService grpcC2DService,
             CurrentUser currentUser)
        {
            _logger = logger;
            _grpcC2DService = grpcC2DService;
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

                var ra = await _grpcC2DService.SendAnyC2DRequest<ClientHeartbeatDto, RequestAnswer>(dto);
                if (ra.ReturnCode != ReturnCode.Ok)
                    _logger.LogError(Logs.Client, $"Failed to send heartbeat! {ra.ErrorMessage}");
                Thread.Sleep(TimeSpan.FromMilliseconds(_heartbeatRate));
            }
            _logger.LogInfo(Logs.Client, @"Leaving Heartbeats...");
        }
    }
}
