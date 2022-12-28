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
        _rtuRepo = rtuRepo;
        _rtuOccupations = rtuOccupations;
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };


    public override async Task<c2rResponse> SendCommand(c2rCommand command, ServerCallContext context)
    {
        try
        {
            var request = Deserialize(command.Json);
            if (request == null)
                return CreateBadResponse(ReturnCode.FailedDeserializeJson);

            var client = _clientCollection.Get(request.ClientConnectionId);
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
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"rtuAddress {rtuAddress}");

            string? responseJson = request.RtuMaker == RtuMaker.IIT
                ? await TransferCommand(rtuAddress, command.Json)
                : JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.NotImplementedYet), JsonSerializerSettings);

            return responseJson == null 
                ? new c2rResponse(){ Json = JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.D2RGrpcOperationError), JsonSerializerSettings)  } 
                : new c2rResponse(){ Json = responseJson };
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
            Json = JsonConvert.SerializeObject(new RequestAnswer(returnCode)
            { RtuOccupationState = currentState }, JsonSerializerSettings)
        };
    }

    private async Task<string?> TransferCommand(string rtuAddress, string commandContent)
    {
        var rtuUri = $"http://{rtuAddress}";
        using var grpcChannelRtu = GrpcChannel.ForAddress(rtuUri);
        _logger.Log(LogLevel.Debug, Logs.DataCenter.ToInt(), $"GrpcChannel for {rtuUri}");
        var grpcClientRtu = new d2r.d2rClient(grpcChannelRtu);
        _logger.Log(LogLevel.Debug, Logs.DataCenter.ToInt(), $"Command content {commandContent}");

        var rtuCommand = new d2rCommand() { Json = commandContent };

        try
        {
            d2rResponse response = await grpcClientRtu.SendCommandAsync(rtuCommand);
            _logger.Log(LogLevel.Debug, Logs.DataCenter.ToInt(), "Got gRPC response from RTU");
            return response.Json;

        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "TransferCommand: " + e.Message);
            if (e.InnerException != null)
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "InnerException: " + e.InnerException.Message);

            return null;
        }
    }
}