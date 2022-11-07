using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
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

    public override async Task<DcGrpcResponse> SendCommand(DcGrpcCommand rtuGrpcCommand, ServerCallContext context)
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

        return new DcGrpcResponse() { Json = JsonConvert.SerializeObject(r) };
    }

    private async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
    {
        await Task.Delay(1);
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "InitializeRtu serverGrpcCommand received");
        return new RtuInitializedDto();
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