using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class R2DService : R2D.R2DBase
{
    private readonly ILogger<R2DService> _logger;
    private readonly IitRtuMessagesProcessor _iitRtuMessagesProcessor;
    private readonly RtuStationsRepository _rtuStationsRepository;
    private readonly GrpcToClient _grpcToClient;

    public R2DService(ILogger<R2DService> logger,
        IitRtuMessagesProcessor iitRtuMessagesProcessor, RtuStationsRepository rtuStationsRepository,
        GrpcToClient grpcToClient)
    {
        _logger = logger;
        _iitRtuMessagesProcessor = iitRtuMessagesProcessor;
        _rtuStationsRepository = rtuStationsRepository;
        _grpcToClient = grpcToClient;
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    public override async Task<R2DGrpcResponse> SendCommand(R2DGrpcCommand r2DGrpcCommand, ServerCallContext context)
    {
        object? o = JsonConvert.DeserializeObject(r2DGrpcCommand.Json, JsonSerializerSettings);

        object r;
        switch (o)
        {
            case RtuChecksChannelDto dto: r = await RegisterHeartbeat(dto); break;
            case ClientMeasurementResultDto dto: r = await NotifyClientMeasurementDone(dto); break;
            case MonitoringResultDto dto: r = await ProcessMonitoringResult(dto); break;
            case BopStateChangedDto dto: r = await ProcessBopStateChanges(dto); break;
            case CurrentMonitoringStepDto dto: r = await TransmitCurrentMonitoringStep(dto); break;
            default: r = new RequestAnswer(ReturnCode.Error); break;
        }

        return new R2DGrpcResponse() { Json = JsonConvert.SerializeObject(r) };
    }

    private async Task<RequestAnswer> RegisterHeartbeat(RtuChecksChannelDto dto)
    {
        var result = await _rtuStationsRepository.RegisterRtuHeartbeatAsync(dto);
        return new RequestAnswer(result == 1 ? ReturnCode.Ok : ReturnCode.Error);
    }

    private async Task<RequestAnswer> NotifyClientMeasurementDone(ClientMeasurementResultDto dto)
    {
        await Task.Delay(1);
        _logger.Info(Logs.DataCenter, $"Client measurement {dto.ClientMeasurementId.First6()} done");
        return await _grpcToClient.SendRequest(dto);
    }

    private async Task<RequestAnswer> ProcessMonitoringResult(MonitoringResultDto dto)
    {
        _logger.Info(Logs.DataCenter, $"Monitoring Result from RTU {dto.RtuId} received");
        await _iitRtuMessagesProcessor.ProcessMonitoringResult(dto);
        return new RequestAnswer(ReturnCode.Ok);
    }

    private async Task<RequestAnswer> ProcessBopStateChanges(BopStateChangedDto dto)
    {
        _logger.Info(Logs.DataCenter, $"Monitoring Result from RTU {dto.RtuId} received");
        await _iitRtuMessagesProcessor.ProcessBopStateChanges(dto);
        return new RequestAnswer(ReturnCode.Ok);
    }

    private async Task<RequestAnswer> TransmitCurrentMonitoringStep(CurrentMonitoringStepDto dto)
    {
        await Task.Delay(1);
        return await _grpcToClient.SendRequest(dto);
    }
}