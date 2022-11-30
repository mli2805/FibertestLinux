using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public class OtdrManager
    {
        private readonly ILogger<OtdrManager> _logger;
        private readonly Interop _interop;
        private readonly SerialPortManager _serialPort;

        public OtdrManager(ILogger<OtdrManager> logger, Interop interop, SerialPortManager serialPort)
        {
            _logger = logger;
            _interop = interop;
            _serialPort = serialPort;
        }

        public async Task<RtuInitializedDto> InitializeOtdr(string otdrIp)
        {
            if (!_interop.InitDll())
                return new RtuInitializedDto(ReturnCode.OtdrInitializationCannotInitializeDll);

            Thread.Sleep(300);
            if (! await ConnectOtdr(otdrIp))
                return new RtuInitializedDto(ReturnCode.FailedToConnectOtdr);

            var result = new RtuInitializedDto(ReturnCode.Ok);

            result.Mfid = _interop.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoMfid);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"MFID = {result.Mfid}");
            result.Mfsn = _interop.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoMfsn);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"MFSN = {result.Mfsn}");
            result.Omid = _interop.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoOmid);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"OMID = {result.Omid}");
            result.Omsn = _interop.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoOmsn);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"OMSN = {result.Omsn}");

            return result;
        }

        private async Task<bool>  ConnectOtdr(string ipAddress)
        {
            await Task.Delay(1);
            var tcpPort = 1500; // read from ini
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Connecting to OTDR {ipAddress}:{tcpPort}...");
            var isOtdrConnected = _interop.InitOtdr(ConnectionTypes.Tcp, ipAddress, tcpPort);
            if (!isOtdrConnected)
                _serialPort.ShowOnLedDisplay(LedDisplayCode.ErrorConnectOtdr);
            return isOtdrConnected;
        }

        public async Task<RtuInitializedDto> InitializeOtau(RtuInitializedDto result)
        {
            await Task.Delay(1);
            return result;
        }

        public async Task<bool> DisconnectOtdr(string ipAddress)
        {
            await Task.Delay(1);
            var tcpPort = 1500; // read from ini
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Disconnecting OTDR {ipAddress}...");
            var result = _interop.InitOtdr(ConnectionTypes.FreePort, ipAddress, tcpPort);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), 
                result ? "OTDR disconnected successfully!" : "Failed to disconnect OTDR!");
            return result;
        }
    }
}
