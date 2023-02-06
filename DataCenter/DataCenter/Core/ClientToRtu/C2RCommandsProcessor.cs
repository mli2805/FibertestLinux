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
        if (command is CheckRtuConnectionDto checkDto)
            return await CheckRtuConnection(checkDto, rtuAddressTuple.Item2!);

        var resultJson = command.RtuMaker == RtuMaker.IIT
            ? await _clientToIitRtuTransmitter
                .TransferCommand(rtuAddressTuple.Item2!,
                    JsonConvert.SerializeObject(command, JsonSerializerSettings))
            : JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.NotImplementedYet),
                JsonSerializerSettings);
        ///////////////////////////////////////////////////////////////////

        // resultJson could be changed while post processing
        return await _rtuResponseApplier.ApplyResponse(command, resultJson);
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
        if (command is CheckRtuConnectionDto checkDto)
        {
            rtuAddress = checkDto.NetAddress.ToStringA();
        }
        else if (command is InitializeRtuDto dto)
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
                return new Tuple<string?, string?>(JsonConvert.SerializeObject(
                    new RequestAnswer(ReturnCode.RtuNotFound), JsonSerializerSettings), null);

            rtuAddress = rtuStation.GetRtuAvailableAddress();
            if (rtuAddress == null)
                return new Tuple<string?, string?>(JsonConvert.SerializeObject(
                    new RequestAnswer(ReturnCode.RtuNotAvailable), JsonSerializerSettings), null);
        }
        _logger.LogInfo(Logs.DataCenter, $"rtuAddress {rtuAddress}");

        return new Tuple<string?, string?>(null, rtuAddress);
    }

    private async Task<string> CheckRtuConnection(CheckRtuConnectionDto dto, string rtuAddress)
    {
        RtuConnectionCheckedDto? result = null;
        if (dto.NetAddress.Port == (int)TcpPorts.RtuListenTo)
        {
            string res = await _clientToIitRtuTransmitter.TransferCommand(rtuAddress,
                JsonConvert.SerializeObject(dto, JsonSerializerSettings));
            result = JsonConvert.DeserializeObject<RtuConnectionCheckedDto>(res);
        }

        if (dto.NetAddress.Port == (int)TcpPorts.RtuVeexListenTo)
        {
            //TODO check veex
            result = new RtuConnectionCheckedDto(ReturnCode.Error) { NetAddress = dto.NetAddress };
        }

        if (dto.NetAddress.Port == -1)
        {
            dto.NetAddress.Port = (int)TcpPorts.RtuListenTo;
            rtuAddress = dto.NetAddress.ToStringA();
            string res = await _clientToIitRtuTransmitter.TransferCommand(rtuAddress,
                JsonConvert.SerializeObject(dto, JsonSerializerSettings));
            result = JsonConvert.DeserializeObject<RtuConnectionCheckedDto>(res);
            if (result != null && !result.IsConnectionSuccessful)
            {
                dto.NetAddress.Port = (int)TcpPorts.RtuListenTo;
                rtuAddress = dto.NetAddress.ToStringA();
                //TODO check veex
                _logger.LogError(Logs.DataCenter, $"Check for VEEX RTU {rtuAddress} is not implemented yet");
                // only if failed
                dto.NetAddress.Port = -1;
                result = new RtuConnectionCheckedDto(ReturnCode.Error) { NetAddress = dto.NetAddress };
            }
        }

        result ??= new RtuConnectionCheckedDto(ReturnCode.D2RGrpcOperationError) { NetAddress = dto.NetAddress };
        if (!result.IsConnectionSuccessful)
        {
            result.IsPingSuccessful =
                Pinger.Ping(dto.NetAddress.IsAddressSetAsIp ? dto.NetAddress.Ip4Address : dto.NetAddress.HostName);
        }

        return JsonConvert.SerializeObject(result, JsonSerializerSettings);
    }
}