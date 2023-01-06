using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public class OtdrManager
{
    private readonly ILogger<OtdrManager> _logger;
    private readonly InterOpWrapper _interOpWrapper;
    private readonly SerialPortManager _serialPort;

    private const string CharonIp = "192.168.88.101";  // read from ini
    private const int OtdrTcpPort = 1500;  // read from ini

    public OtdrManager(ILogger<OtdrManager> logger, InterOpWrapper interOpWrapper, SerialPortManager serialPort)
    {
        _logger = logger;
        _interOpWrapper = interOpWrapper;
        _serialPort = serialPort;

    }

    public async Task<RtuInitializedDto> InitializeOtdr()
    {
        if (!_interOpWrapper.InitDll())
            return new RtuInitializedDto(ReturnCode.OtdrInitializationCannotInitializeDll);

        Thread.Sleep(300);
        if (! await ConnectOtdr())
            return new RtuInitializedDto(ReturnCode.FailedToConnectOtdr);

        var result = new RtuInitializedDto(ReturnCode.Ok);

        result.OtdrAddress = new NetAddress(CharonIp, OtdrTcpPort);
        result.Mfid = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoMfid);
        _logger.LLog(Logs.RtuManager.ToInt(), $"MFID = {result.Mfid}");
        result.Mfsn = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoMfsn);
        _logger.LLog(Logs.RtuManager.ToInt(), $"MFSN = {result.Mfsn}");
        result.Omid = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoOmid);
        _logger.LLog(Logs.RtuManager.ToInt(), $"OMID = {result.Omid}");
        result.Omsn = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoOmsn);
        _logger.LLog(Logs.RtuManager.ToInt(), $"OMSN = {result.Omsn}");

        return result;
    }

    private async Task<bool>  ConnectOtdr()
    {
        await Task.Delay(1);
        _logger.LLog(Logs.RtuManager.ToInt(), $"Connecting to OTDR {CharonIp}:{OtdrTcpPort}...");
        var isOtdrConnected = _interOpWrapper.InitOtdr(ConnectionTypes.Tcp, CharonIp, OtdrTcpPort);
        if (!isOtdrConnected)
            _serialPort.ShowOnLedDisplay(LedDisplayCode.ErrorConnectOtdr);
        return isOtdrConnected;
    }

    public async Task<bool> DisconnectOtdr()
    {
        await Task.Delay(1);
        _logger.LLog(Logs.RtuManager.ToInt(), $"Disconnecting OTDR {CharonIp}...");
        var result = _interOpWrapper.InitOtdr(ConnectionTypes.FreePort, CharonIp, OtdrTcpPort);
        _logger.LLog(Logs.RtuManager.ToInt(), 
            result ? "OTDR disconnected successfully!" : "Failed to disconnect OTDR!");
        return result;
    }
}