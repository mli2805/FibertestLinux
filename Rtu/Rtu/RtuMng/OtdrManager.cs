using Fibertest.Dto;
using Fibertest.Utils;
using System.Reflection;

namespace Fibertest.Rtu;

public partial class OtdrManager
{
    private readonly ILogger<OtdrManager> _logger;
    private readonly InterOpWrapper _interOpWrapper;
    private readonly SerialPortManager _serialPort;

    private readonly string _iitOtdrFolder;
    private readonly string _charonIp;
    private readonly int _otdrTcpPort;

    public OtdrManager(IWritableConfig<RtuConfig> config, ILogger<OtdrManager> logger,
        InterOpWrapper interOpWrapper)
    {
        _logger = logger;
        _interOpWrapper = interOpWrapper;
        _serialPort = new SerialPortManager();
        _serialPort.Initialize(config.Value.Charon, logger);

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var basePath = Path.GetDirectoryName(assemblyLocation) ?? "";
        _iitOtdrFolder = basePath + "/OtdrMeasEngine";
        _charonIp = config.Value.General.CharonIp;
        _otdrTcpPort = config.Value.General.OtdrTcpPort;
    }

    public RtuInitializedDto InitializeOtdr()
    {
        try
        {
            if (!RestoreEtc())
                return new RtuInitializedDto(ReturnCode.OtdrInitializationCannotInitializeDll);
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, "Failed to restore etc: " + e.Message);
            return new RtuInitializedDto(ReturnCode.OtdrInitializationCannotInitializeDll);
        }

        if (!_interOpWrapper.InitDll())
            return new RtuInitializedDto(ReturnCode.OtdrInitializationCannotInitializeDll);

        Thread.Sleep(300);
        if (!ConnectOtdr())
            return new RtuInitializedDto(ReturnCode.FailedToConnectOtdr);

        var result = new RtuInitializedDto(ReturnCode.Ok);

        result.OtdrAddress = new NetAddress(_charonIp, _otdrTcpPort);
        result.Mfid = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoMfid);
        _logger.Info(Logs.RtuManager, $"MFID = {result.Mfid}");
        result.Mfsn = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoMfsn);
        _logger.Info(Logs.RtuManager, $"MFSN = {result.Mfsn}");
        result.Omid = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoOmid);
        _logger.Info(Logs.RtuManager, $"OMID = {result.Omid}");
        result.Omsn = _interOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetOtdrInfoOmsn);
        _logger.Info(Logs.RtuManager, $"OMSN = {result.Omsn}");

        return result;
    }

    private bool RestoreEtc()
    {
        var destinationPath = Path.Combine(_iitOtdrFolder, @"etc");
        if (!Directory.Exists(destinationPath))
        {
            _logger.Error(Logs.RtuManager, $"Can't work without <{destinationPath}> folder!");
            return false;
        }
        var sourcePath = Path.Combine(_iitOtdrFolder, "etc_default");
        if (!Directory.Exists(sourcePath))
        {
            _logger.Error(Logs.RtuManager, $"Can't work without <{sourcePath}> folder!");
            return false;
        }
        var files = Directory.GetFiles(sourcePath);
        foreach (var file in files)
        {
            var sourceFile = Path.GetFileName(file);
            var destFile = Path.Combine(destinationPath, sourceFile);
            File.Copy(file, destFile, true);
        }
        _logger.Info(Logs.RtuManager, "ETC folder restored successfully!");
        return true;
    }

    public bool ConnectOtdr()
    {
        _logger.Info(Logs.RtuManager, $"Connecting to OTDR {_charonIp}:{_otdrTcpPort}...");
        var isOtdrConnected = _interOpWrapper.InitOtdr(ConnectionTypes.Tcp, _charonIp, _otdrTcpPort);
        if (!isOtdrConnected)
            _serialPort.ShowOnLedDisplay(LedDisplayCode.ErrorConnectOtdr);
        return isOtdrConnected;
    }

    public bool DisconnectOtdr()
    {
        _logger.Info(Logs.RtuManager, $"Disconnecting OTDR {_charonIp}...");
        return _interOpWrapper.InitOtdr(ConnectionTypes.FreePort, _charonIp, _otdrTcpPort);
    }
}