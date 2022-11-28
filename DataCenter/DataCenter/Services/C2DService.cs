using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public class C2DService : c2d.c2dBase
    {
        private readonly ILogger<C2DService> _logger;
        private readonly ClientCollection _clientCollection;
        private readonly ClientGrpcRequestExecutor _clientGrpcRequestExecutor;

        public C2DService(ILogger<C2DService> logger, ClientCollection clientCollection,
            ClientGrpcRequestExecutor clientGrpcRequestExecutor)
        {
            _logger = logger;
            _clientCollection = clientCollection;
            _clientGrpcRequestExecutor = clientGrpcRequestExecutor;
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };


        public override async Task<c2dResponse> SendCommand(c2dCommand command, ServerCallContext context)
        {
            try
            {
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "C2D command received");
                var request = Deserialize(command.Json);
                if (request == null)
                    return CreateBadResponse(ReturnCode.FailedDeserializeJson);

                if (!(request is RegisterClientDto))
                {
                    var client = _clientCollection.Get(request.ConnectionId);
                    if (client == null)
                        return CreateBadResponse(ReturnCode.UnAuthorizedAccess);
                }
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Client sent {request.What} request");
              
                var response = await _clientGrpcRequestExecutor.ExecuteRequest(request);
                return new c2dResponse()
                { Json = JsonConvert.SerializeObject(response, JsonSerializerSettings) };
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), e.Message);
                return CreateBadResponse(ReturnCode.D2RGrpcOperationError);
            }
        }

        private BaseRequest? Deserialize(string json)
        {
            return JsonConvert.DeserializeObject(json, JsonSerializerSettings) switch
            {
                RegisterClientDto dto => dto,
                RegisterHeartbeatDto dto => dto,
                SetRtuOccupationDto dto => dto,
                _ => null
            };
        }

        private c2dResponse CreateBadResponse(ReturnCode returnCode)
        {
            return new c2dResponse
            {
                Json = JsonConvert.SerializeObject(new RequestAnswer(returnCode), JsonSerializerSettings)
            };
        }
    }
}
