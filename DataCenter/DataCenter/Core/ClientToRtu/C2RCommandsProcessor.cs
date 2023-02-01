using Fibertest.Dto;
using Fibertest.Graph;
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
    private readonly Model _writeModel;
    private readonly BaseRefsCheckerOnServer _baseRefsCheckerOnServer;
    private readonly RtuStationsRepository _rtuStationsRepository;
    private readonly ClientToIitRtuTransmitter _clientToIitRtuTransmitter;
    private readonly RtuResponseApplier _rtuResponseApplier;

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    private readonly DoubleAddress _serverDoubleAddress;
    public C2RCommandsProcessor(IWritableConfig<DataCenterConfig> config, ILogger<C2RCommandsProcessor> logger,
        Model writeModel, BaseRefsCheckerOnServer baseRefsCheckerOnServer,
        RtuStationsRepository rtuStationsRepository, ClientToIitRtuTransmitter clientToIitRtuTransmitter,
        RtuResponseApplier rtuResponseApplier)
    {
        _logger = logger;
        _writeModel = writeModel;
        _baseRefsCheckerOnServer = baseRefsCheckerOnServer;
        _rtuStationsRepository = rtuStationsRepository;
        _clientToIitRtuTransmitter = clientToIitRtuTransmitter;
        _rtuResponseApplier = rtuResponseApplier;

        _serverDoubleAddress = config.Value.General.ServerDoubleAddress;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <returns>json to send by gRPC</returns>
    public async Task<string> SendCommand<T>(T command) where T : BaseRtuRequest
    {
        var rtuAddressTuple = await GetRtuAddress(command);
        if (rtuAddressTuple.Item1 != null) // problems with RTU address
            return rtuAddressTuple.Item1;

        var validationResult = ValidateCommand(command);
        if (validationResult != null)
            return validationResult;

        ///////////////////////////////////////////////////////////////////
        var resultJson = command.RtuMaker == RtuMaker.IIT
            ? await _clientToIitRtuTransmitter
                .TransferCommand(rtuAddressTuple.Item2!,
                    JsonConvert.SerializeObject(command, JsonSerializerSettings))
            : JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.NotImplementedYet),
                JsonSerializerSettings);
        ///////////////////////////////////////////////////////////////////

        // resultJson could be changed while post processing
        return await PostProcessResult(command, resultJson);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <returns>null if it is all right</returns>
    private string? ValidateCommand<T>(T command) where T : BaseRtuRequest
    {
        if (command is AssignBaseRefsDto dto)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == dto.TraceId);
            if (trace == null)
                return JsonConvert.SerializeObject(new BaseRefAssignedDto
                {
                    ErrorMessage = "trace not found",
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed
                }, JsonSerializerSettings);

            var checkResult = _baseRefsCheckerOnServer.AreBaseRefsAcceptable(dto.BaseRefs, trace);
            if (checkResult != null)
                return JsonConvert.SerializeObject(checkResult, JsonSerializerSettings);
        }

        return null;
    }

    /// <summary>
    ///  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <returns>Item1 is json response ifError, Item2 is RtuAddress</returns>
    private async Task<Tuple<string?, string?>> GetRtuAddress<T>(T command) where T : BaseRtuRequest
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
                return new Tuple<string?, string?>(JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.RtuNotFound), JsonSerializerSettings), null);

            rtuAddress = rtuStation.GetRtuAvailableAddress();
            if (rtuAddress == null)
                return new Tuple<string?, string?>(JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.RtuNotAvailable), JsonSerializerSettings), null);
        }
        _logger.LogInfo(Logs.DataCenter, $"rtuAddress {rtuAddress}");

        return new Tuple<string?, string?>(null, rtuAddress);
    }

    private async Task<string> PostProcessResult<T>(T command, string jsonResult) where T : BaseRtuRequest
    {
        switch (command)
        {
            case InitializeRtuDto dto:
                return await _rtuResponseApplier.ApplyRtuInitializationResult(dto, jsonResult);
            case AttachOtauDto dto:
                return await _rtuResponseApplier.ApplyOtauAttachmentResult(dto, jsonResult);
            case DetachOtauDto dto:
                return await _rtuResponseApplier.ApplyOtauDetachmentResult(dto, jsonResult);
            case AssignBaseRefsDto dto:
                return await _rtuResponseApplier.ApplyBaseRefsAssignmentResult(dto, jsonResult);
            default:
                return JsonConvert
                    .SerializeObject(new RequestAnswer(ReturnCode.Error) { ErrorMessage = "Unknown command" },
                        JsonSerializerSettings);
        }
    }
}