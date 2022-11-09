using Fibertest.Dto;
using Fibertest.Rtu;
using Fibertest.Utils;
using Grpc.Core;
using Grpc.Net.Client;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class ServerService : Server.ServerBase
{
    private readonly ILogger<ServerService> _logger;
    private RtuRepo _rtuRepo;

    public ServerService(ILogger<ServerService> logger, RtuRepo rtuRepo)
    {
        _logger = logger;
        _rtuRepo = rtuRepo;
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };


    public override async Task<C2RTransferResponse> SendCommand(C2RTransferCommand command, ServerCallContext context)
    {
        var response = await TransferCommand(command);
        return new C2RTransferResponse() { Json = JsonConvert.SerializeObject(response, JsonSerializerSettings) };
    }

    private async Task<BaseRtuReply> TransferCommand(C2RTransferCommand command)
    {
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "Transfer command received");
        var rtuAddress = _rtuRepo.GetRtuAddresses(Guid.Parse(command.RtuGuid));
        if (rtuAddress == null)
            return new BaseRtuReply() { ReturnCode = ReturnCode.NoSuchRtu };

        var rtuUri = $"http://{rtuAddress}:{(int)TcpPorts.RtuListenTo}";
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