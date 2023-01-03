using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class C2RService : c2r.c2rBase
{
    private readonly ILogger<C2RService> _logger;
    private readonly ClientCollection _clientCollection;
    private readonly RtuOccupations _rtuOccupations;
    private readonly RtuStationsRepository _rtuStationsRepository;
    private readonly IntermediateClass _intermediateClass;
    private readonly ClientToIitRtuTransmitter _clientToIitRtuTransmitter;

    public C2RService(ILogger<C2RService> logger, ClientCollection clientCollection,
        RtuOccupations rtuOccupations, RtuStationsRepository rtuStationsRepository,
        IntermediateClass intermediateClass, ClientToIitRtuTransmitter clientToIitRtuTransmitter)
    {
        _logger = logger;
        _clientCollection = clientCollection;
        _rtuOccupations = rtuOccupations;
        _rtuStationsRepository = rtuStationsRepository;
        _intermediateClass = intermediateClass;
        _clientToIitRtuTransmitter = clientToIitRtuTransmitter;
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

            if (request is InitializeRtuDto dto)
            {
                var res = await _intermediateClass.InitializeRtuAsync(dto);
                return new c2rResponse() { Json = JsonConvert.SerializeObject(res, JsonSerializerSettings) };
            }

            var rtuStation = await _rtuStationsRepository.GetRtuStation(request.RtuId);
            if (rtuStation == null)
                return CreateBadResponse(ReturnCode.RtuNotFound);

            string? rtuAddress = rtuStation.GetRtuAvailableAddress();
            if (rtuAddress == null)
                return CreateBadResponse(ReturnCode.RtuNotAvailable);
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"rtuAddress {rtuAddress}");

            string responseJson = request.RtuMaker == RtuMaker.IIT
                ? await _clientToIitRtuTransmitter.TransferCommand(rtuAddress, command.Json)
                : JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.NotImplementedYet), JsonSerializerSettings);

            _rtuOccupations.TrySetOccupation(request.RtuId, RtuOccupation.None, client.UserName, out RtuOccupationState? _);

            return new c2rResponse() { Json = responseJson };
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
}