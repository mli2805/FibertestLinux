using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Core;
using Newtonsoft.Json;

namespace Fibertest.Rtu
{
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

            object r;
            switch (o)
            {
                case InitializeRtuDto dto: r = await InitializeRtu(dto); break;
                case StopMonitoringDto _: r = await StopMonitoring(); break;
                case AttachOtauDto dto: r = await AttachOtau(dto); break;
                case FreeOtdrDto _: r = await FreeOtdr(); break;
                default: r = new RequestAnswer(ReturnCode.Error); break;
            }

            return new d2rResponse() { Json = JsonConvert.SerializeObject(r) };
        }

        private async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
        {
            await Task.Delay(1);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "InitializeRtu d2RCommand received");
            var result = await _rtuManager.InitializeRtu();

            if (result.IsInitialized)
            {
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "RTU initialized successfully!");
                return new RtuInitializedDto(ReturnCode.RtuInitializedSuccessfully)
                {
                    RtuId = dto.RtuId,
                };
            }
            else
            {
                _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "Failed initialize RTU!");
                return new RtuInitializedDto(ReturnCode.RtuInitializationError)
                {
                    RtuId = dto.RtuId,
                };
            }
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

        private async Task<RequestAnswer> FreeOtdr()
        {
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "FreeOtdr d2RCommand received");
            return await _rtuManager.FreeOtdr();
        }
    }
}
