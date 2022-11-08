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

    public ServerService(ILogger<ServerService> logger)
    {
        _logger = logger;
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    public override async Task<C2DGrpcResponse> SendCommand(C2DGrpcCommand rtuGrpcCommand, ServerCallContext context)
    {
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "we are in here");
        object? o = JsonConvert.DeserializeObject(rtuGrpcCommand.Json, JsonSerializerSettings);

        object r;
        switch (o)
        {
            case InitializeRtuDto dto: r = await InitializeRtu(dto); break;
            case StopMonitoringDto _: r = await StopMonitoring(); break;
            case AttachOtauDto dto: r = await AttachOtau(dto); break;
            case FreeOtdrDto dto: r = await FreeOtdr(dto); break;
            default: r = new BaseRtuReply(); break;
        }

        return new C2DGrpcResponse() { Json = JsonConvert.SerializeObject(r, JsonSerializerSettings) };
    }

    private async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
    {
        await Task.Delay(1);
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "InitializeRtu serverGrpcCommand received");

        var rtuAddress = "localhost";
        // var rtuAddress = "192.168.96.56";
        var rtuUri = $"http://{rtuAddress}:{(int)TcpPorts.RtuListenTo}";
        using var grpcChannelRtu = GrpcChannel.ForAddress(rtuUri);
        var grpcClientRtu = new RtuManager.RtuManagerClient(grpcChannelRtu);

        var command = new RtuGrpcCommand() { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };
        try
        {
            RtuGrpcResponse response = await grpcClientRtu.SendCommandAsync(command);
            var result = JsonConvert.DeserializeObject<RtuInitializedDto>(response.Json);
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(),
                result == null ? "RTU response is null" : $"RTU response is {result.IsInitialized}");
            return result ?? new RtuInitializedDto();
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "InitializeRtu: " + e.Message);
            return new RtuInitializedDto() { ReturnCode = ReturnCode.D2RWcfOperationError };
        }
    }

    private async Task<BaseRtuReply> StopMonitoring()
    {
        await Task.Delay(1);
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "StopMonitoring serverGrpcCommand received");
        return new BaseRtuReply();
    }

    private async Task<OtauAttachedDto> AttachOtau(AttachOtauDto dto)
    {
        await Task.Delay(1);
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "AttachOtau serverGrpcCommand received");
        return new OtauAttachedDto();
    }

    private async Task<BaseRtuReply> FreeOtdr(FreeOtdrDto dto)
    {
        await Task.Delay(1);
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "FreeOtdr serverGrpcCommand received");
        return new BaseRtuReply();
    }
}