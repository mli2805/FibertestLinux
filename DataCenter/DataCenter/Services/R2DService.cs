using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public class R2DService : R2D.R2DBase
    {
        private readonly ILogger<R2DService> _logger;

        public R2DService(ILogger<R2DService> logger)
        {
            _logger = logger;
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        public override async Task<R2DGrpcResponse> SendCommand(R2DGrpcCommand r2DGrpcCommand, ServerCallContext context)
        {
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "we are in here");
            object? o = JsonConvert.DeserializeObject(r2DGrpcCommand.Json, JsonSerializerSettings);

            object r;
            switch (o)
            {
                case RtuChecksChannelDto dto: r = await RegisterHeartbeat(dto); break;
                case MonitoringResultDto dto: r = await ProcessMonitoringResult(dto); break;
                default: r = new BaseReply(); break;
            }

            return new R2DGrpcResponse() { Json = JsonConvert.SerializeObject(r) };
        }

        private async Task<BaseReply> RegisterHeartbeat(RtuChecksChannelDto dto)
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Command Register Heartbeat from RTU {dto.RtuId} received");
            return new BaseReply();
        }

        private async Task<BaseReply> ProcessMonitoringResult(MonitoringResultDto dto)
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Monitoring Result from RTU {dto.RtuId} received");
            return new BaseReply();
        }
    }
}
