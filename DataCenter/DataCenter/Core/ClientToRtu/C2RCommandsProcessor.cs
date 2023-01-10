using Fibertest.Dto;
using Fibertest.Utils;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

/// <summary>
/// All clients to RTU commands
///     desktop via gRPC
///     web via HTTP
/// should come here
/// 
/// </summary>
public class C2RCommandsProcessor
{
    private readonly ILogger<C2RCommandsProcessor> _logger;
    private readonly RtuStationsRepository _rtuStationsRepository;
    private readonly ClientToIitRtuTransmitter _clientToIitRtuTransmitter;
    private readonly RtuResponseApplier _rtuResponseApplier;

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    private readonly DoubleAddress _serverDoubleAddress;
    public C2RCommandsProcessor(IWritableOptions<ServerGeneralConfig> config, ILogger<C2RCommandsProcessor> logger,
        RtuStationsRepository rtuStationsRepository, ClientToIitRtuTransmitter clientToIitRtuTransmitter,
        RtuResponseApplier rtuResponseApplier)
    {
        _logger = logger;
        _rtuStationsRepository = rtuStationsRepository;
        _clientToIitRtuTransmitter = clientToIitRtuTransmitter;
        _rtuResponseApplier = rtuResponseApplier;

        _serverDoubleAddress = config.Value.ServerDoubleAddress;
    }

    public async Task<string> SendCommand<T>(T command) where T : BaseRtuRequest // where TResult : RequestAnswer, new()
    {
        var preProcessResult = await PreProcessCommand(command);
        if (preProcessResult.Item1.ReturnCode != ReturnCode.Ok) // problems with RTU address
            return await PostProcessResult(
                command, JsonConvert.SerializeObject(preProcessResult.Item1, JsonSerializerSettings));

        var resultJson = command.RtuMaker == RtuMaker.IIT
            ? await _clientToIitRtuTransmitter
                .TransferCommand(preProcessResult.Item2!,
                    JsonConvert.SerializeObject(command, JsonSerializerSettings))
            : JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.NotImplementedYet),
                JsonSerializerSettings);
        return await PostProcessResult(command, resultJson);
    }

    private async Task<Tuple<RequestAnswer, string?>> PreProcessCommand<T>(T command) where T : BaseRtuRequest
    {
        string? rtuAddress;
        if (command is InitializeRtuDto dto)
        {
            dto.ServerAddresses = _serverDoubleAddress.Clone();
            if (!dto.RtuAddresses.HasReserveAddress)
                // if RTU has no reserve address it should not send to server's reserve address
                // (it is an ideological requirement)
                dto.ServerAddresses.HasReserveAddress = false;

            rtuAddress = dto.RtuAddresses.Main.ToStringA();
        }
        else
        {
            var rtuStation = await _rtuStationsRepository.GetRtuStation(command.RtuId);
            if (rtuStation == null)
                return new Tuple<RequestAnswer, string?>(new RequestAnswer(ReturnCode.RtuNotFound), null);

            rtuAddress = rtuStation.GetRtuAvailableAddress();
            if (rtuAddress == null)
                return new Tuple<RequestAnswer, string?>(new RequestAnswer(ReturnCode.RtuNotAvailable), null);
        }
        _logger.LLog(Logs.DataCenter.ToInt(), $"rtuAddress {rtuAddress}");

        return new Tuple<RequestAnswer, string?>(new RequestAnswer(ReturnCode.Ok), rtuAddress);
    }

    private async Task<string> PostProcessResult<T>(T command, string jsonResult)
        where T : BaseRtuRequest
    {
        switch (command)
        {
            case InitializeRtuDto dto:
                return await _rtuResponseApplier.ApplyRtuInitializationResult(dto, jsonResult);
            case AttachOtauDto dto:
                return await _rtuResponseApplier.ApplyOtauAttachmentResult(dto, jsonResult);
            default:
                return JsonConvert
                    .SerializeObject(new RequestAnswer(ReturnCode.Error) { ErrorMessage = "Unknown command" },
                        JsonSerializerSettings);
        }
    }
}