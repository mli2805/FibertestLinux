using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class C2DService : c2d.c2dBase
{
    private readonly ILogger<C2DService> _logger;
    private readonly ClientCollection _clientCollection;
    private readonly Model _writeModel;
    private readonly ClientGrpcRequestExecutor _clientGrpcRequestExecutor;
    private readonly EventStoreService _eventStoreService;

    public C2DService(ILogger<C2DService> logger, ClientCollection clientCollection, Model writeModel,
        ClientGrpcRequestExecutor clientGrpcRequestExecutor, EventStoreService eventStoreService)
    {
        _logger = logger;
        _clientCollection = clientCollection;
        _writeModel = writeModel;
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

                string username;
                string clientIp;
                if (cmd is ApplyLicense applyLicenseCmd)
                {
                    var user = _writeModel.Users.FirstOrDefault(u => u.Title == applyLicenseCmd.UserName);
                    if (user == null)
                        return CreateBadResponse(ReturnCode.NoSuchUserOrWrongPassword);
                    username = user.Title;
                    clientIp = "";
                }
                else
                {

                    var client = _clientCollection.Get(command.ClientConnectionId);
                    if (client == null)
                        return CreateBadResponse(ReturnCode.NoSuchUserOrWrongPassword);
                    username = client.UserName;
                    clientIp = client.ClientIp;
                }

                var res = await _eventStoreService.SendCommand(cmd, username, clientIp);
                var answer = new RequestAnswer(string.IsNullOrEmpty(res) ? ReturnCode.Ok : ReturnCode.Error);
                return new c2dResponse { Json = JsonConvert.SerializeObject(answer, JsonSerializerSettings) };
            }

            var request = Deserialize(command.Json);
            if (request == null)
                return CreateBadResponse(ReturnCode.FailedDeserializeJson);

            if (request is RegisterClientDto || request is CheckServerConnectionDto)
            {
                // it is registration process, do not check connection ID
            }
            else
            {
                var client = _clientCollection.Get(request.ClientConnectionId);
                if (client == null)
                    return CreateBadResponse(ReturnCode.UnAuthorizedAccess);
            }

            if (request.What != "GetEvents")
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
            CheckServerConnectionDto dto => dto,
            RegisterClientDto dto => dto,
            ClientHeartbeatDto dto => dto,
            SetRtuOccupationDto dto => dto,

            GetDiskSpaceDto dto => dto,
            GetEventsDto dto => dto,
            GetSerializedModelParamsDto dto => dto,
            GetModelPortionDto dto => dto,

            ChangeDcConfigDto dto => dto,
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