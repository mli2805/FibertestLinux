using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.Extensions.Options;

namespace Fibertest.Rtu;

public class OtdrManager
{
    private readonly IOptions<CharonConfig> _config;
    private readonly ILogger<OtdrManager> _logger;
    private readonly InterOpWrapper _interOpWrapper;
    private readonly SerialPortManager _serialPort;

    private const string CharonIp = "192.168.88.101";

    private const int OtdrTcpPort = 1500;

    public OtdrManager(IOptions<CharonConfig> config, ILogger<OtdrManager> logger, InterOpWrapper interOpWrapper, SerialPortManager serialPort)
    {
        _config = config;
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
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"MFID = {result.Mfid}");
        result.Mfsn = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoMfsn);
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"MFSN = {result.Mfsn}");
        result.Omid = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoOmid);
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"OMID = {result.Omid}");
        result.Omsn = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoOmsn);
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"OMSN = {result.Omsn}");

        return result;
    }

    private async Task<bool>  ConnectOtdr()
    {
        await Task.Delay(1);
        var tcpPort = 1500; // read from ini
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Connecting to OTDR {CharonIp}:{tcpPort}...");
        var isOtdrConnected = _interOpWrapper.InitOtdr(ConnectionTypes.Tcp, CharonIp, tcpPort);
        if (!isOtdrConnected)
            _serialPort.ShowOnLedDisplay(LedDisplayCode.ErrorConnectOtdr);
        return isOtdrConnected;
    }

    public async Task<RtuInitializedDto> InitializeOtau(RtuInitializedDto result)
    {
        await Task.Delay(1);
        var mainCharon = new Charon(new NetAddress(CharonIp, 23), true, _config, _logger, _serialPort);
        var res = mainCharon.InitializeOtauRecursively();
        if (res == mainCharon.NetAddress)
            return new RtuInitializedDto(ReturnCode.OtauInitializationError);

        result.Serial = mainCharon.Serial;
        result.OwnPortCount = mainCharon.OwnPortCount;
        result.FullPortCount = mainCharon.FullPortCount;
        result.Children = mainCharon.GetChildrenDto();

        result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;

        mainCharon.ShowMessageMeasurementPort();

        return result;
    }

    public async Task<bool> DisconnectOtdr()
    {
        await Task.Delay(1);
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Disconnecting OTDR {CharonIp}...");
        var result = _interOpWrapper.InitOtdr(ConnectionTypes.FreePort, CharonIp, OtdrTcpPort);
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), 
            result ? "OTDR disconnected successfully!" : "Failed to disconnect OTDR!");
        return result;
    }
}