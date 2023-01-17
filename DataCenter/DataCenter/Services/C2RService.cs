using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class C2RService : c2r.c2rBase
{
    private readonly ILogger<C2RService> _logger;
    private readonly ClientCollection _clientCollection;
    private readonly RtuOccupations _rtuOccupations;
    private readonly C2RCommandsProcessor _c2RCommandsProcessor;

    public C2RService(ILogger<C2RService> logger, ClientCollection clientCollection,
        RtuOccupations rtuOccupations, 
        C2RCommandsProcessor c2RCommandsProcessor)
    {
        _logger = logger;
        _clientCollection = clientCollection;
        _rtuOccupations = rtuOccupations;
        _c2RCommandsProcessor = c2RCommandsProcessor;
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };


    public override async Task<c2rResponse> SendCommand(c2rCommand command, ServerCallContext context)
    {
        try
        {
            var request = (BaseRtuRequest?)JsonConvert.DeserializeObject(command.Json, JsonSerializerSettings);
            if (request == null)
                return CreateBadResponse(ReturnCode.FailedDeserializeJson);

            var client = _clientCollection.Get(request.ClientConnectionId);
            if (client == null)
                return CreateBadResponse(ReturnCode.UnAuthorizedAccess);

            _logger.LogInfo(Logs.DataCenter,
                $"Client {client} sent {request.What} RTU {request.RtuId.First6()} request");

            if (!_rtuOccupations.TrySetOccupation(
                    request.RtuId, request.Why, client.UserName, out RtuOccupationState? currentState))
                return CreateBadResponse(ReturnCode.RtuIsBusy, currentState);

            ////////////////////////////////////////////////////////////////////////////
            var responseJson = await _c2RCommandsProcessor.SendCommand(request);
            ////////////////////////////////////////////////////////////////////////////

            _rtuOccupations.TrySetOccupation(request.RtuId, RtuOccupation.None, client.UserName, out RtuOccupationState? _);

            return new c2rResponse() { Json = responseJson };
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.DataCenter, e.Message);
            return CreateBadResponse(ReturnCode.D2RGrpcOperationError);
        }
    }


    private c2rResponse CreateBadResponse(ReturnCode returnCode, RtuOccupationState? currentState = null)
    {
        return new c2rResponse
        {
            Json = JsonConvert.SerializeObject(new RequestAnswer(returnCode)
            { RtuOccupationState = currentState }, JsonSerializerSettings)
        };
    }
}