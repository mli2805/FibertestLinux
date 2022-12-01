using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.Rtu;

public class RtuGrpcService : d2r.d2rBase
{
    private readonly ILogger<RtuGrpcService> _logger;
    private readonly RtuManager _rtuManager;

    public RtuGrpcService(ILogger<RtuGrpcService> logger, RtuManager rtuManager)
    {
        _logger = logger;
        _rtuManager = rtuManager;
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    public override async Task<d2rResponse> SendCommand(d2rCommand d2RCommand, ServerCallContext context)
    {
        _logger.Log(LogLevel.Information, Logs.RtuService.ToInt(), "we are in here");
        object? o = JsonConvert.DeserializeObject(d2RCommand.Json, JsonSerializerSettings);

        object result;
        switch (o)
        {
            case InitializeRtuDto dto: result = await _rtuManager.InitializeRtu(dto); break;
            case StopMonitoringDto _: result = await StopMonitoring(); break;
            case AttachOtauDto dto: result = await AttachOtau(dto); break;
            case FreeOtdrDto _: result = await _rtuManager.FreeOtdr(); break;
            default: result = new RequestAnswer(ReturnCode.Error); break;
        }

        return new d2rResponse() { Json = JsonConvert.SerializeObject(result) };
    }

    private async Task<RequestAnswer> StopMonitoring()
    {
        await Task.Delay(1);
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "StopMonitoring d2RCommand received");
        return new RequestAnswer(ReturnCode.Ok);
    }

    private async Task<OtauAttachedDto> AttachOtau(AttachOtauDto dto)
    {
        await Task.Delay(1);
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),
            $"Command to attach OTAU {dto.NetAddress?.ToStringASpace ?? "no address!"} received");
        return new OtauAttachedDto(ReturnCode.Ok);
    }
}