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
        object? o = JsonConvert.DeserializeObject(d2RCommand.Json, JsonSerializerSettings);
        if (o == null)
            return new d2rResponse()
            { Json = JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.FailedDeserializeJson)) };
        var request = (BaseRtuRequest)o;
        _logger.LogInfo(Logs.RtuService, $"request {request.What} received");

        object result;
        switch (o)
        {
            case CheckRtuConnectionDto dto: 
                result = new RtuConnectionCheckedDto(ReturnCode.Ok) { NetAddress = dto.NetAddress.Clone() }; break;
            case InitializeRtuDto dto: result = await _rtuManager.InitializeRtu(dto); break;
            case ApplyMonitoringSettingsDto dto: result = await _rtuManager.ApplyMonitoringSettings(dto); break;
            case StopMonitoringDto _: result = await _rtuManager.StopMonitoring(); break;
            case AttachOtauDto dto: result = await _rtuManager.AttachOtau(dto); break;
            case DetachOtauDto dto: result = await _rtuManager.DetachOtau(dto); break;
            case AssignBaseRefsDto dto: result = await _rtuManager.SaveBaseRefs(dto); break;
            case FreeOtdrDto _: result = await _rtuManager.FreeOtdr(); break;
            default: result = new RequestAnswer(ReturnCode.Error); break;
        }

        return new d2rResponse() { Json = JsonConvert.SerializeObject(result) };
    }


}