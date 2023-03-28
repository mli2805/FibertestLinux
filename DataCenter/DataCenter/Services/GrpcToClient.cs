using Fibertest.Dto;
using Fibertest.Graph;
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

        private readonly ILogger<ClientGrpcRequestExecutor> _logger;
        private readonly ClientCollection _clientCollection;

        public GrpcToClient(ILogger<ClientGrpcRequestExecutor> logger,
            ClientCollection clientCollection)
        {
            _logger = logger;
            _clientCollection = clientCollection;
        }

        public async Task<RequestAnswer> SendRequest(object o)
        {
            switch (o)
            {
                case ClientMeasurementResultDto dto: return await SendClientMeasurementResult(dto);
                case CurrentMonitoringStepDto dto: return await SendCurrentMeasurementStep(dto);
                default: return new RequestAnswer(ReturnCode.Error);
            }

        }

        private async Task<RequestAnswer> SendClientMeasurementResult(ClientMeasurementResultDto dto)
        {
            var clientStation = _clientCollection.Get(dto.ClientConnectionId);
            if (clientStation == null) return new RequestAnswer(ReturnCode.Error);

            var commandContent = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
            return await TransferToClient(clientStation.ClientIp, commandContent);
        }

        private async Task<RequestAnswer> SendCurrentMeasurementStep(CurrentMonitoringStepDto dto)
        {
            var commandContent = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
            foreach (var clientStation in _clientCollection.Clients.Values)
            {
                var transferResult = await TransferToClient(clientStation.ClientIp, commandContent);
                _logger.Info(Logs.DataCenter, $"transfer result is {transferResult.ReturnCode.GetLocalizedString()}");
            }

            return new RequestAnswer(ReturnCode.Ok);
        }

        private async Task<RequestAnswer> TransferToClient(string clientIp, string commandContent)
        {
            var clientUri = $"http://{clientIp}:{(int)TcpPorts.ClientListenTo}";
            using var grpcChannelToClient = GrpcChannel.ForAddress(clientUri);
            _logger.Debug(Logs.DataCenter, $"GrpcChannel to client {clientUri}");
            var grpcClientToClient = new toClient.toClientClient(grpcChannelToClient);

            var clientCommand = new toClientCommand { Json = commandContent };

            try
            {
                toClientResponse response = await grpcClientToClient.SendCommandAsync(clientCommand);
                _logger.Debug(Logs.DataCenter, $"Got gRPC response from client: {response.Json}");
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
