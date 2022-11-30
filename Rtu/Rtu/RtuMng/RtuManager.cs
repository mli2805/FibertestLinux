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

        public RtuManager(IOptions<RtuConfig> config, ILogger<RtuManager> logger,
            SerialPortManager serialPortManager, OtdrManager otdrManager)
        {
            _logger = logger;
            _serialPortManager = serialPortManager;
            _otdrManager = otdrManager;

            _otdrIp = config.Value.OtdrIp ?? "192.168.88.101";
        }

        public async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto? dto = null)
        {
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "RtuManager: InitializeRtu");
            var res = _serialPortManager.ResetCharon();
            if (res != ReturnCode.Ok)
                return new RtuInitializedDto(res);

            _serialPortManager.ShowOnLedDisplay(LedDisplayCode.Connecting);

            var result = await _otdrManager.InitializeOtdr(_otdrIp);
            if (result.ReturnCode != ReturnCode.Ok)
                return result;

            return await _otdrManager.InitializeOtau(result);
        }

        public async Task<RequestAnswer> FreeOtdr()
        {
            var res = await _otdrManager.DisconnectOtdr(_otdrIp);
            return new RequestAnswer(res ? ReturnCode.Ok : ReturnCode.Error);
        }
    }
}
