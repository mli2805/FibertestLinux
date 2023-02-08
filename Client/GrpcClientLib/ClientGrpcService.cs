using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using GrpsClientLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fibertest.GrpcClientLib
{
    public class ClientGrpcService : toClient.toClientBase
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        private readonly ILogger<ClientGrpcService> _logger;

        public ClientGrpcService(ILogger<ClientGrpcService> logger)
        {
            _logger = logger;
        }

        public override async Task<toClientResponse> SendCommand(toClientCommand command, ServerCallContext context)
        {
            try
            {
                var request = Deserialize(command.Json);
                if (request == null)
                    return CreateBadResponse(ReturnCode.FailedDeserializeJson);

                switch (request)
                {
                //TODO: process request - raise event ?
                    case ClientMeasurementResultDto _:
                        _logger.LogInfo(Logs.DataCenter, $"Client gets ClientMeasurementResult dto from DC"); break;
                    case CurrentMonitoringStepDto _:
                        _logger.LogInfo(Logs.DataCenter, $"Client gets CurrentMonitoringStepDto dto from DC"); break;
                }



                return new toClientResponse()
                { Json = JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.Ok), JsonSerializerSettings) };
            }
            catch (Exception e)
            {
                await Task.Delay(0);
                _logger.LogError(Logs.Client, e.Message);
                return CreateBadResponse(ReturnCode.ToClientGrpcOperationError);
            }
        }

        private BaseRequest? Deserialize(string json)
        {
            return JsonConvert.DeserializeObject(json, JsonSerializerSettings) switch
            {
                CurrentMonitoringStepDto dto => dto,
                ClientMeasurementResultDto dto => dto,
                _ => null
            };
        }

        private toClientResponse CreateBadResponse(ReturnCode returnCode)
        {
            return new toClientResponse
            {
                Json = JsonConvert.SerializeObject(new RequestAnswer(returnCode), JsonSerializerSettings)
            };
        }
    }
}
