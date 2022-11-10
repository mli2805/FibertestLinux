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
    private RtuRepo _rtuRepo;

    public C2RService(ILogger<C2RService> logger, RtuRepo rtuRepo)
    {
        _logger = logger;
        _rtuRepo = rtuRepo;
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };


    public override async Task<c2rResponse> SendCommand(c2rCommand command, ServerCallContext context)
    {
        try
        {
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "Transfer command received");
            var response = await TransferCommand(command);
            return new c2rResponse() { Json = JsonConvert.SerializeObject(response, JsonSerializerSettings) };
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), e.Message);
            throw;
        }
    }

    private async Task<BaseRtuReply> TransferCommand(c2rCommand command)
    {
        var rtuAddress = _rtuRepo.GetRtuAvailableAddress(Guid.Parse(command.RtuGuid));
        if (rtuAddress == null)
            return new BaseRtuReply() { ReturnCode = ReturnCode.NoSuchRtu };

        var rtuUri = $"http://{rtuAddress}";
        using var grpcChannelRtu = GrpcChannel.ForAddress(rtuUri);
        var grpcClientRtu = new RtuManager.RtuManagerClient(grpcChannelRtu);
        var rtuCommand = new RtuGrpcCommand() { Json = command.Json };

        try
        {
            RtuGrpcResponse response = await grpcClientRtu.SendCommandAsync(rtuCommand);
            var result = JsonConvert.DeserializeObject<RtuInitializedDto>(response.Json);
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(),
                result == null ? "RTU response is null" : $"RTU response is {result.IsInitialized}");
            return result ?? new BaseRtuReply();
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "TransferCommand: " + e.Message);
            return new RtuInitializedDto() { ReturnCode = ReturnCode.D2RWcfOperationError };
        }
    }
}