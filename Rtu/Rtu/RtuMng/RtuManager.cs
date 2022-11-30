using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.Extensions.Options;

namespace Fibertest.Rtu
{
    public class RtuManager
    {
        private readonly ILogger<RtuManager> _logger;
        private readonly SerialPortManager _serialPortManager;
        private readonly OtdrManager _otdrManager;

        private readonly string _otdrIp;
        private readonly string _otauIp;

        public RtuManager(IOptions<RtuConfig> config, ILogger<RtuManager> logger,
            SerialPortManager serialPortManager, OtdrManager otdrManager)
        {
            _logger = logger;
            _serialPortManager = serialPortManager;
            _otdrManager = otdrManager;

            _otdrIp = config.Value.OtdrIp ?? "192.168.88.101";
            _otauIp = config.Value.OtauIp ?? "192.168.88.101";
        }

        public async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto? dto = null)
        {
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "RtuManager: InitializeRtu");
            var res = _serialPortManager.ResetCharon();
            if (res != ReturnCode.Ok)
                return new RtuInitializedDto(res);

            _serialPortManager.ShowOnLedDisplay(LedDisplayCode.Connecting); // "Connecting..."

            var result = await _otdrManager.InitializeOtdr(_otdrIp);
            if (result.ReturnCode != ReturnCode.Ok)
                return result;

            var result2 = await _otdrManager.InitializeOtau(result, _otauIp);
            if (result2.IsInitialized)
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "RTU initialized successfully!");
            else
                _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "Failed initialize RTU!");
            return result2;
        }

        public async Task<RequestAnswer> FreeOtdr()
        {
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "RtuManager: FreeOtdr");
            var res = await _otdrManager.DisconnectOtdr(_otdrIp);
            return new RequestAnswer(res ? ReturnCode.Ok : ReturnCode.Error);
        }
    }
}
