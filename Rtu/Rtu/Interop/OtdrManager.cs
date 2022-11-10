using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public class OtdrManager
    {
        private readonly ILogger<OtdrManager> _logger;
        private readonly Interop _interop;

        public OtdrManager(ILogger<OtdrManager> logger, Interop interop)
        {
            _logger = logger;
            _interop = interop;
        }

        public bool InitDll()
        {
            return _interop.InitDll();
        }

        public bool ConnectOtdr(string ipAddress)
        {
            var tcpPort = 1500; // read from ini
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Connecting to OTDR {ipAddress}:{tcpPort}...");
            var isOtdrConnected = _interop.InitOtdr(ConnectionTypes.Tcp, ipAddress, tcpPort);
            // if (!isOtdrConnected)
            //     LedDisplay.Show(_iniFile, _rtuLogger, LedDisplayCode.ErrorConnectOtdr);
            return isOtdrConnected;
        }

        public bool DisconnectOtdr(string ipAddress)
        {
            var tcpPort = 1500; // read from ini
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Disconnecting OTDR {ipAddress}...");
            var result = _interop.InitOtdr(ConnectionTypes.FreePort, ipAddress, tcpPort);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), 
                result ? "OTDR disconnected successfully!" : "Failed to disconnect OTDR!");
            return result;
        }
    }
}
