using Fibertest.Dto;
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

        private async Task<RequestAnswer> TransferToClient(string clientIp, string commandContent)
        {
            var clientUri = $"http://{clientIp}:{(int)TcpPorts.ClientListenTo}";
            using var grpcChannelToClient = GrpcChannel.ForAddress(clientUri);
            _logger.LogDebug(Logs.DataCenter, $"GrpcChannel to client {clientUri}");
            var grpcClientToClient = new toClient.toClientClient(grpcChannelToClient);

            var clientCommand = new toClientCommand { Json = commandContent };

            try
            {
                toClientResponse response = await grpcClientToClient.SendCommandAsync(clientCommand);
                _logger.LogDebug(Logs.DataCenter, $"Got gRPC response from client: {response.Json}");
                var result = JsonConvert.DeserializeObject<RequestAnswer>(response.Json, JsonSerializerSettings);
                return result ?? new RequestAnswer(ReturnCode.FailedDeserializeJson);
            }
            catch (Exception e)
            {
                _logger.LogError(Logs.DataCenter, "TransferToClient: " + e.Message);
                if (e.InnerException != null)
                    _logger.LogError(Logs.DataCenter, "InnerException: " + e.InnerException.Message);

                return new RequestAnswer(ReturnCode.ToClientGrpcOperationError) { ErrorMessage = e.Message };
            }
        }
    }
}
