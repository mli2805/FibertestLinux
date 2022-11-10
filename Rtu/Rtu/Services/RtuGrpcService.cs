using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.Rtu
{
    public class RtuGrpcService : d2r.d2rBase
    {
        private readonly ILogger<RtuGrpcService> _logger;
        private readonly OtdrManager _otdrManager;

        public RtuGrpcService(ILogger<RtuGrpcService> logger, OtdrManager otdrManager)
        {
            _logger = logger;
            _otdrManager = otdrManager;
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        public override async Task<d2rResponse> SendCommand(d2rCommand d2RCommand, ServerCallContext context)
        {
            _logger.Log(LogLevel.Information, Logs.RtuService.ToInt(), "we are in here");
            object? o = JsonConvert.DeserializeObject(d2RCommand.Json, JsonSerializerSettings);

            object r;
            switch (o)
            {
                case InitializeRtuDto dto: r = await InitializeRtu(dto); break;
                case StopMonitoringDto _: r = await StopMonitoring(); break;
                case AttachOtauDto dto: r = await AttachOtau(dto); break;
                case FreeOtdrDto _: r = await FreeOtdr(); break;
                default: r = new BaseRtuReply(); break;
            }

            return new d2rResponse() { Json = JsonConvert.SerializeObject(r) };
        }

        private async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "InitializeRtu d2RCommand received");
            var result = _otdrManager.InitDll() && _otdrManager.ConnectOtdr("192.168.88.101");

            if (result)
            {
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "RTU initialized successfully!");
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializedSuccessfully,
                    RtuId = dto.RtuId,
                    Serial = "13579"
                };
            }
            else
            {
                _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "Failed initialize RTU!");
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError,
                    RtuId = dto.RtuId,
                };
            }
        }

        private async Task<BaseRtuReply> StopMonitoring()
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "StopMonitoring d2RCommand received");
            return new BaseRtuReply();
        }

        private async Task<OtauAttachedDto> AttachOtau(AttachOtauDto dto)
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),
                $"Command to attach OTAU {dto.NetAddress?.ToStringASpace ?? "no address!"} received");
            return new OtauAttachedDto();
        }

        private async Task<BaseRtuReply> FreeOtdr()
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "FreeOtdr d2RCommand received");
            var result = _otdrManager.DisconnectOtdr("192.168.88.101");
            return new BaseRtuReply() { ReturnCode = result ? ReturnCode.Ok : ReturnCode.Error };
        }
    }
}
