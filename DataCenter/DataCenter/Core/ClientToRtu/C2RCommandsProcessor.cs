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
    private readonly SorFileRepository _sorFileRepository;
    private readonly ClientToIitRtuTransmitter _clientToIitRtuTransmitter;
    private readonly RtuResponseApplier _rtuResponseApplier;

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    private readonly DoubleAddress _serverDoubleAddress;
    public C2RCommandsProcessor(IWritableConfig<DataCenterConfig> config, ILogger<C2RCommandsProcessor> logger,
        Model writeModel, BaseRefsCheckerOnServer baseRefsCheckerOnServer,
        RtuStationsRepository rtuStationsRepository, SorFileRepository sorFileRepository,
        ClientToIitRtuTransmitter clientToIitRtuTransmitter,
        RtuResponseApplier rtuResponseApplier)
    {
        _logger = logger;
        _writeModel = writeModel;
        _baseRefsCheckerOnServer = baseRefsCheckerOnServer;
        _rtuStationsRepository = rtuStationsRepository;
        _sorFileRepository = sorFileRepository;
        _clientToIitRtuTransmitter = clientToIitRtuTransmitter;
        _rtuResponseApplier = rtuResponseApplier;

        _serverDoubleAddress = config.Value.General.ServerDoubleAddress;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <returns>RTU returns json by gRPC,
    /// so return json to return to desktop client by gRPC
    /// for web client parsing should be done so far
    /// (TODO refactoring in web client: accept json)
    /// </returns>
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

        string? jsonToTransmit = await ConvertAndSerialize(command);
        if (jsonToTransmit == null)
        {
            var result = new BaseRefAssignedDto() { ReturnCode = ReturnCode.Error, ErrorMessage = "RTU or trace not found!" };
            return JsonConvert.SerializeObject(result, JsonSerializerSettings);
        }

        var resultJson = command.RtuMaker == RtuMaker.IIT
                 ? await _clientToIitRtuTransmitter
                     .TransferCommand(rtuAddressTuple.Item2!, jsonToTransmit)
                 : JsonConvert
                     .SerializeObject(new RequestAnswer(ReturnCode.NotImplementedYet), JsonSerializerSettings);
        ///////////////////////////////////////////////////////////////////

        // resultJson could be changed while post processing
        return await _rtuResponseApplier.ApplyResponse(command, resultJson);
    }

    /// <summary>
    /// If commands are AttachTraceDto || ReSendBaseRefsDto
    /// we need to fetch sor files from DB
    /// and send them to RTU in AssignBaseRefsDto
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <returns></returns>
    private async Task<string?> ConvertAndSerialize<T>(T command)
    {
        if (command is AttachTraceDto attachTraceDto)
        {
            var assignBaseRefsDto = await _writeModel
                .CreateAssignBaseRefsDto(attachTraceDto.TraceId, attachTraceDto.OtauPortDto,
                    attachTraceDto.MainOtauPortDto, _sorFileRepository);
            return LogCommandReturnJson(assignBaseRefsDto);
        }
        if (command is ReSendBaseRefsDto reSendBaseRefsDto)
        {
            var assignBaseRefsDto = await _writeModel
                .CreateAssignBaseRefsDto(reSendBaseRefsDto.TraceId, reSendBaseRefsDto.OtauPortDto,
                    reSendBaseRefsDto.MainOtauPortDto, _sorFileRepository);
            return LogCommandReturnJson(assignBaseRefsDto);
        }
        return LogCommandReturnJson(command);
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
        _logger.Info(Logs.DataCenter, $"rtuAddress {rtuAddress}");

        return new Tuple<string?, string?>(null, rtuAddress);
    }

    private async Task<string> CheckRtuConnection(CheckRtuConnectionDto dto, string rtuAddress)
    {
        RtuConnectionCheckedDto? result = null;
        if (dto.NetAddress.Port == (int)TcpPorts.RtuListenToGrpc)
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
            dto.NetAddress.Port = (int)TcpPorts.RtuListenToGrpc;
            rtuAddress = dto.NetAddress.ToStringA();
            string res = await _clientToIitRtuTransmitter.TransferCommand(rtuAddress,
                JsonConvert.SerializeObject(dto, JsonSerializerSettings));
            result = JsonConvert.DeserializeObject<RtuConnectionCheckedDto>(res);
            if (result != null && !result.IsConnectionSuccessful)
            {
                dto.NetAddress.Port = (int)TcpPorts.RtuListenToGrpc;
                rtuAddress = dto.NetAddress.ToStringA();
                //TODO check veex
                _logger.Error(Logs.DataCenter, $"Check for VEEX RTU {rtuAddress} is not implemented yet");
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

    /// If command contains SOR we should not log bytes array
    private string? LogCommandReturnJson<T>(T? command)
    {
        if (command == null)
        {
            _logger.Error(Logs.DataCenter, "Failed to create AssignBaseRefsDto");
            return null;
        }

        var jsonToTransmit = JsonConvert.SerializeObject(command, JsonSerializerSettings);

        switch (command)
        {
            // for not logging sor bytes
            case AssignBaseRefsDto dto:
                var print = JsonConvert.SerializeObject(dto.ShallowCopy(), JsonSerializerSettings);
                _logger.Debug(Logs.DataCenter, $"Command content {print}");
                _logger.Debug(Logs.DataCenter, $"  {dto.BaseRefs.Count} base refs");
                break;
            default:
                _logger.Debug(Logs.DataCenter, $"Command content {jsonToTransmit}");
                break;
        }

        return jsonToTransmit;
    }
}