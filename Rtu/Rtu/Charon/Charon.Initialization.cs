using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.Extensions.Options;

namespace Fibertest.Rtu;

public partial class Charon
{
    private readonly IOptions<CharonConfig> _config;
    private readonly ILogger _logger;
    private readonly SerialPortManager _serialPort;

    private readonly int _connectionTimeout;
    private readonly int _readTimeout;
    private readonly int _writeTimeout;
    private readonly int _pauseBetweenCommands;
    public int CharonIniSize { get; set; }
    public NetAddress NetAddress { get; set; }
    public string? Serial { get; set; }
    public int OwnPortCount { get; set; }
    public int FullPortCount { get; set; }
    public bool IsMainCharon { get; set; }
    public bool IsBopSupported { get; set; } // for main charon
    public bool IsOk { get; set; }

    public Dictionary<int, Charon> Children { get; set; } = new();

    public string LastErrorMessage { get; set; } = string.Empty;
    public string LastAnswer { get; set; } = string.Empty;
    public bool IsLastCommandSuccessful { get; set; }

    private ILogger<Charon> _lll;

    public Charon(NetAddress netAddress, bool isMainCharon, IOptions<CharonConfig> config, ILogger logger, SerialPortManager serialPort)
    {
        _config = config;
        _logger = logger;
        _serialPort = serialPort;
        _connectionTimeout = config.Value.ConnectionTimeout != 0 ? config.Value.ConnectionTimeout : 5;
        _readTimeout = config.Value.ReadTimeout != 0 ? config.Value.ReadTimeout : 2;
        _writeTimeout = config.Value.WriteTimeout != 0 ? config.Value.WriteTimeout : 2;
        _pauseBetweenCommands = config.Value.PauseBetweenCommandsMs != 0 ? config.Value.PauseBetweenCommandsMs : 200;
        NetAddress = netAddress;
        IsMainCharon = isMainCharon;

        var lf = new LoggerFactory();
        _lll = new Logger<Charon>(lf);
    }

    /// <summary>
    /// Initialized OTAU recursively
    /// </summary>
    /// <returns>null if initialization is successful, damaged OTAU address otherwise</returns>
    public NetAddress? InitializeOtauRecursively()
    {
        _lll.LogError(Logs.RtuManager, "local logger");

        _logger.LogInfo(Logs.RtuManager, $"Initializing OTAU on {NetAddress.ToStringA()}");
        Children = new Dictionary<int, Charon>();

        Serial = GetSerial();
        if (!IsLastCommandSuccessful)
        {
            _serialPort.ShowOnLedDisplay(LedDisplayCode.ErrorConnectOtau);
            LastErrorMessage = $"Get Serial for {NetAddress.ToStringA()} error {LastErrorMessage}";
            _logger.LogInfo(Logs.RtuManager, LastErrorMessage);
            return NetAddress;
        }
        Serial = Serial.Substring(0, Serial.Length - 2);
        _logger.LogInfo(Logs.RtuManager, $"Serial {Serial}");

        OwnPortCount = GetOwnPortCount();
        FullPortCount = OwnPortCount;
        if (!IsLastCommandSuccessful)
        {
            LastErrorMessage = $"Get own port count error {LastErrorMessage}";
            _logger.LogError(Logs.RtuManager, LastErrorMessage);
            return NetAddress;
        }
        _logger.LogInfo(Logs.RtuManager, $"Own port count  {OwnPortCount}");
        IsOk = true;

        if (IsMainCharon)
        {
            CharonIniSize = GetIniSize();
            IsBopSupported = CharonIniSize > 0;
            var extendedPorts = GetExtendedPorts();
            if (extendedPorts == null)
                return NetAddress;

            bool isBopRemoved = false;
            foreach (var p in extendedPorts.Keys.ToList())
            {
                if (p > OwnPortCount)
                {
                    // боп подключен к порту которого нет (может поменялся рту)
                    extendedPorts.Remove(p);
                    isBopRemoved = true;
                    continue;
                }

                var expendedPort = extendedPorts[p];
                var childCharon = new Charon(expendedPort, false, _config, _logger, _serialPort);
                Children.Add(p, childCharon); // even if it broken it should be in list

                var childSerial = childCharon.GetSerial();
                if (!IsLastCommandSuccessful || childSerial == "")
                {
                    _serialPort.ShowOnLedDisplay(LedDisplayCode.ErrorConnectOtau);
                    childCharon.LastErrorMessage = $"Get Serial for {expendedPort.ToStringA()} error {LastErrorMessage}";
                    _logger.LogError(Logs.RtuManager, childCharon.LastErrorMessage);
                }
                else
                {
                    childCharon.Serial = childSerial.Substring(0, childSerial.Length - 2);
                    if (childCharon.InitializeOtauRecursively() != null)
                    {
                        _serialPort.ShowOnLedDisplay(LedDisplayCode.ErrorConnectBop);
                        IsLastCommandSuccessful = true; // child initialization shouldn't break full process
                        childCharon.LastErrorMessage = LastErrorMessage = $"Child charon {expendedPort.ToStringA()} initialization failed";
                        _logger.LogError(Logs.RtuManager, childCharon.LastErrorMessage);
                        continue;
                    }
                    FullPortCount += childCharon.FullPortCount;
                }
            }

            if (isBopRemoved)
                RewriteIni(extendedPorts);
        }
        _logger.LogInfo(Logs.RtuManager, $"Full port count  {FullPortCount}");
        _logger.LogInfo(Logs.RtuManager, $"OTAU {Serial} initialized successfully.   {OwnPortCount}/{FullPortCount}.");
        return null;
    }

    public Dictionary<int, OtauDto> GetChildrenDto()
    {
        return Children.ToDictionary(
            pair => pair.Key,
            pair => new OtauDto()
                { Serial = pair.Value.Serial, OwnPortCount = pair.Value.OwnPortCount, NetAddress = pair.Value.NetAddress, IsOk = pair.Value.IsOk });
    }
}