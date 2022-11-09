using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public class OtdrManager
    {
        private readonly ILogger<OtdrManager> _logger;

        public OtdrManager(ILogger<OtdrManager> logger)
        {
            _logger = logger;
        }

        public bool ConnectOtdr(string ipAddress)
        {
            var tcpPort = 1500;
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Connecting to OTDR {ipAddress}:{tcpPort}...");
            var isOtdrConnected = Interop.InitOtdr(ConnectionTypes.Tcp, ipAddress, tcpPort, _logger);
            // if (!isOtdrConnected)
            //     LedDisplay.Show(_iniFile, _rtuLogger, LedDisplayCode.ErrorConnectOtdr);
            return isOtdrConnected;
        }

        public void DisconnectOtdr(string ipAddress)
        {
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Disconnecting OTDR {ipAddress}...");
            var result = Interop.InitOtdr(ConnectionTypes.FreePort, ipAddress, 1500, _logger);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), 
                result ? "OTDR disconnected successfully!" : "Failed to disconnect OTDR!");
        }
    }
}
