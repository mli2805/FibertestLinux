﻿using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using GrpsClientLib;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public class GrpcToClient
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        private readonly ILogger<GrpcToClient> _logger;
        private readonly ClientCollection _clientCollection;

        public GrpcToClient(ILogger<GrpcToClient> logger, ClientCollection clientCollection)
        {
            _logger = logger;
            _clientCollection = clientCollection;
        }

        public async Task<RequestAnswer> SendRequest(object o)
        {
            switch (o)
            {
                case ClientMeasurementResultDto dto: 
                    return await SendClientMeasurementResult(dto);
                case CurrentMonitoringStepDto dto: 
                    return await SendToAllDesktopClients(dto);
                case DbOptimizationProgressDto dto: 
                    return await SendToAllDesktopClients(dto);
                default: 
                    return new RequestAnswer(ReturnCode.Error);
            }
        }

        private async Task<RequestAnswer> SendClientMeasurementResult(ClientMeasurementResultDto dto)
        {
            var commandContent = JsonConvert.SerializeObject(dto, JsonSerializerSettings);

            var clientStation = _clientCollection.Get(dto.ClientConnectionId);
            if (clientStation == null) 
                return new RequestAnswer(ReturnCode.Error);

            return await TransferToClient(clientStation.ClientIp, commandContent);
        }

        private async Task<RequestAnswer> SendToAllDesktopClients(object dto)
        {
            var commandContent = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
            foreach (var clientStation in _clientCollection.Clients.Values)
                await TransferToClient(clientStation.ClientIp, commandContent);

            return new RequestAnswer(ReturnCode.Ok);
        }

        private async Task<RequestAnswer> TransferToClient(string clientIp, string commandContent)
        {
            var clientUri = $"http://{clientIp}:{(int)TcpPorts.ClientListenTo}";
            using var grpcChannelToClient = GrpcChannel.ForAddress(clientUri);
            var grpcClientToClient = new toClient.toClientClient(grpcChannelToClient);

            var clientCommand = new toClientCommand { Json = commandContent };

            try
            {
                toClientResponse response = await grpcClientToClient.SendCommandAsync(clientCommand);
                var result = JsonConvert.DeserializeObject<RequestAnswer>(response.Json, JsonSerializerSettings);
                return result ?? new RequestAnswer(ReturnCode.FailedDeserializeJson);
            }
            catch (Exception e)
            {
                _logger.Error(Logs.DataCenter, "TransferToClient: " + e.Message);
                if (e.InnerException != null)
                    _logger.Error(Logs.DataCenter, "InnerException: " + e.InnerException.Message);

                return new RequestAnswer(ReturnCode.ToClientGrpcOperationError) { ErrorMessage = e.Message };
            }
        }
    }
}
