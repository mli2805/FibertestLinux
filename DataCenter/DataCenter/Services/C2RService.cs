using Fibertest.Dto;
using Fibertest.Rtu;
using Fibertest.Utils;
using Grpc.Core;
using Grpc.Net.Client;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class C2RService : c2r.c2rBase
{
    private readonly ILogger<C2RService> _logger;
    private readonly ClientCollection _clientCollection;
    private readonly RtuRepo _rtuRepo;
    private readonly RtuOccupations _rtuOccupations;

    public C2RService(ILogger<C2RService> logger, ClientCollection clientCollection, RtuRepo rtuRepo, RtuOccupations rtuOccupations)
    {
        _logger = logger;
        _clientCollection = clientCollection;
        _clientCollection.RegisterClientAsync(new RegisterClientDto("client-connection-id") { UserName = "test user", ClientIp = "localhost" }).Wait();
        _rtuRepo = rtuRepo;
        _rtuOccupations = rtuOccupations;
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };


    public override async Task<c2rResponse> SendCommand(c2rCommand command, ServerCallContext context)
    {
        try
        {
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "Transfer command received");
            var request = Deserialize(command.Json);
            if (request == null)
                return CreateBadResponse(ReturnCode.FailedDeserializeJson);

            var client = _clientCollection.Get(request.ConnectionId);
            if (client == null)
                return CreateBadResponse(ReturnCode.UnAuthorizedAccess);

            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(),
                $"Client {client} sent {request.What} RTU {request.RtuId.First6()} request");

            if (!_rtuOccupations.TrySetOccupation(
                    request.RtuId, request.Why, client.UserName, out RtuOccupationState? currentState))
                return CreateBadResponse(ReturnCode.RtuIsBusy, currentState);

            var rtuStation = _rtuRepo.Get(request.RtuId);
            if (rtuStation == null)
                return CreateBadResponse(ReturnCode.RtuNotFound);

            var rtuAddress = rtuStation.GetRtuAvailableAddress();
            if (rtuAddress == null)
                return CreateBadResponse(ReturnCode.RtuNotAvailable);

            var response = request.RtuMaker == RtuMaker.IIT
                ? await TransferCommand(rtuAddress, command.Json)
                : new BaseRtuReply() { ReturnCode = ReturnCode.Ok };
            return new c2rResponse()
                { Json = JsonConvert.SerializeObject(response, JsonSerializerSettings) };
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), e.Message);
            return CreateBadResponse(ReturnCode.D2RGrpcOperationError);
        }
    }

    private BaseRtuRequest? Deserialize(string json)
    {
        return JsonConvert.DeserializeObject(json, JsonSerializerSettings) switch
        {
            InitializeRtuDto dto => dto,
            StopMonitoringDto dto => dto,
            AttachOtauDto dto => dto,
            FreeOtdrDto dto => dto,
            _ => null
        };
    }

    private c2rResponse CreateBadResponse(ReturnCode returnCode, RtuOccupationState? currentState = null)
    {
        return new c2rResponse
        {
            Json = JsonConvert.SerializeObject(new BaseRtuReply
            { ReturnCode = returnCode, RtuOccupationState = currentState }, JsonSerializerSettings)
        };
    }

    private async Task<BaseRtuReply> TransferCommand(string rtuAddress, string commandContent)
    {
        var rtuUri = $"http://{rtuAddress}";
        using var grpcChannelRtu = GrpcChannel.ForAddress(rtuUri);
        var grpcClientRtu = new d2r.d2rClient(grpcChannelRtu);
        var rtuCommand = new d2rCommand() { Json = commandContent };

        try
        {
            d2rResponse response = await grpcClientRtu.SendCommandAsync(rtuCommand);
            var result = JsonConvert.DeserializeObject<RtuInitializedDto>(response.Json);
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(),
                result == null ? "RTU response is null" : $"RTU response is {result.ReturnCode}");
            return result ?? new BaseRtuReply() { ReturnCode = ReturnCode.Error, ErrorMessage = "response is null" };
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "TransferCommand: " + e.Message);
            return new RtuInitializedDto() { ReturnCode = ReturnCode.D2RGrpcOperationError };
        }
    }
}