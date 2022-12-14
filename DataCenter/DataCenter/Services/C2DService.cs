using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class C2DService : c2d.c2dBase
{
    private readonly ILogger<C2DService> _logger;
    private readonly ClientCollection _clientCollection;
    private readonly ClientGrpcRequestExecutor _clientGrpcRequestExecutor;
    private readonly EventStoreService _eventStoreService;

    public C2DService(ILogger<C2DService> logger, ClientCollection clientCollection,
        ClientGrpcRequestExecutor clientGrpcRequestExecutor, EventStoreService eventStoreService)
    {
        _logger = logger;
        _clientCollection = clientCollection;
        _clientGrpcRequestExecutor = clientGrpcRequestExecutor;
        _eventStoreService = eventStoreService;
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };


    public override async Task<c2dResponse> SendCommand(c2dCommand command, ServerCallContext context)
    {
        try
        {
            if (command.IsEventSourcingCommand)
            {
                _logger.LogInfo(Logs.DataCenter, "Event sourcing command");
                var cmd = JsonConvert.DeserializeObject(command.Json, JsonSerializerSettings);
                if (cmd == null)
                    return CreateBadResponse(ReturnCode.FailedDeserializeJson);

                var client = _clientCollection.Get(command.ClientConnectionId);
                if (client == null)
                    return CreateBadResponse(ReturnCode.NoSuchUserOrWrongPassword);

                var res = await _eventStoreService.SendCommand(cmd, client.UserName, client.ClientIp);
                var answer = new RequestAnswer(string.IsNullOrEmpty(res) ? ReturnCode.Ok : ReturnCode.Error);
                return new c2dResponse { Json = JsonConvert.SerializeObject(answer, JsonSerializerSettings) };
            }

            var request = Deserialize(command.Json);
            if (request == null)
                return CreateBadResponse(ReturnCode.FailedDeserializeJson);

            if (!(request is RegisterClientDto))
            {
                var client = _clientCollection.Get(request.ConnectionId);
                if (client == null)
                    return CreateBadResponse(ReturnCode.UnAuthorizedAccess);
            }
            _logger.LogInfo(Logs.DataCenter, $"Client sent {request.What} request");
              
            var response = await _clientGrpcRequestExecutor.ExecuteRequest(request);
            return new c2dResponse { Json = JsonConvert.SerializeObject(response, JsonSerializerSettings) };
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.DataCenter, e.Message);
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