using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.Rtu
{
    public class RtuGrpcService : RtuManager.RtuManagerBase
    {
        private readonly ILogger<RtuGrpcService> _logger;

        public RtuGrpcService(ILogger<RtuGrpcService> logger)
        {
            _logger = logger;
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        public override async Task<RtuGrpcResponse> SendCommand(RtuGrpcCommand rtuGrpcCommand, ServerCallContext context)
        {
            _logger.Log(LogLevel.Information, Logs.RtuService.ToInt(), "we are in here");
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

            return new RtuGrpcResponse() { Json = JsonConvert.SerializeObject(r) };
        }

        private async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "InitializeRtu rtuGrpcCommand received");
            return new RtuInitializedDto();
        }

        private async Task<BaseRtuReply> StopMonitoring()
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "StopMonitoring rtuGrpcCommand received");
            return new BaseRtuReply();
        }

        private async Task<OtauAttachedDto> AttachOtau(AttachOtauDto dto)
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "AttachOtau rtuGrpcCommand received");
            return new OtauAttachedDto();
        }

        private async Task<BaseRtuReply> FreeOtdr(FreeOtdrDto dto)
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "FreeOtdr rtuGrpcCommand received");
            return new BaseRtuReply();
        }
    }
}
