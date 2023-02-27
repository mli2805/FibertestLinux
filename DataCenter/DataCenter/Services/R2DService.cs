﻿using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class R2DService : R2D.R2DBase
{
    private readonly ILogger<R2DService> _logger;
    private readonly RtuStationsRepository _rtuStationsRepository;
    private readonly GrpcToClient _grpcToClient;

    public R2DService(ILogger<R2DService> logger, RtuStationsRepository rtuStationsRepository,
        GrpcToClient grpcToClient)
    {
        _logger = logger;
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
            case CurrentMonitoringStepDto dto: r = await TransmitCurrentMonitoringStep(dto); break;
            default: r = new RequestAnswer(ReturnCode.Error); break;
        }

        return new R2DGrpcResponse() { Json = JsonConvert.SerializeObject(r) };
    }

    private async Task<RequestAnswer> RegisterHeartbeat(RtuChecksChannelDto dto)
    {
        await Task.Delay(1);
        _logger.Info(Logs.DataCenter, $"Command Register Heartbeat from RTU {dto.RtuId} received");
        var result = await _rtuStationsRepository.RegisterRtuHeartbeatAsync(dto);
        return new RequestAnswer(result == 1 ? ReturnCode.Ok : ReturnCode.Error);
    }

    private async Task<RequestAnswer> NotifyClientMeasurementDone(ClientMeasurementResultDto dto)
    {
        await Task.Delay(1);
        _logger.Info(Logs.DataCenter, $"Client measurement {dto.ClientMeasurementId.First6()} done");
        var result = await _grpcToClient.SendRequest(dto);
        if (result.ReturnCode == ReturnCode.Ok) 
            _logger.Info(Logs.DataCenter, "Sent to client successfully!");
        else
            _logger.Error(Logs.DataCenter, "Failed to send to client");
        return result;
    }

    private async Task<RequestAnswer> ProcessMonitoringResult(MonitoringResultDto dto)
    {
        await Task.Delay(1);
        _logger.Info(Logs.DataCenter, $"Monitoring Result from RTU {dto.RtuId} received");
        return new RequestAnswer(ReturnCode.Ok);
    }

    private async Task<RequestAnswer> TransmitCurrentMonitoringStep(CurrentMonitoringStepDto dto)
    {
        await Task.Delay(1);
        _logger.Info(Logs.DataCenter, $"Current monitoring step from RTU {dto.RtuId} received");
        return new RequestAnswer(ReturnCode.Ok);
    }
}