using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

    private readonly IWritableOptions<RtuGeneralConfig> _rtuGeneralConfig;
    private readonly IWritableOptions<MonitoringConfig> _monitoringConfig;
    private readonly IOptions<CharonConfig> _charonConfig;
    private readonly IWritableOptions<RecoveryConfig> _recoveryConfig;
    private readonly ILogger<RtuManager> _logger;
    private readonly SerialPortManager _serialPortManager;
    private readonly InterOpWrapper _interOpWrapper;
    private readonly OtdrManager _otdrManager;
    private readonly GrpcSender _grpcSender;

    private Guid _id;
    private Charon _mainCharon = null!;


    public RtuManager(IWritableOptions<RtuGeneralConfig> rtuGeneralConfig, IWritableOptions<MonitoringConfig> monitoringConfig,
        IOptions<CharonConfig> charonConfig, IWritableOptions<RecoveryConfig> recoveryConfig,
        ILogger<RtuManager> logger,
        SerialPortManager serialPortManager, InterOpWrapper interOpWrapper, OtdrManager otdrManager,
        GrpcSender grpcSender)
    {
        _rtuGeneralConfig = rtuGeneralConfig;
        _monitoringConfig = monitoringConfig;
        _charonConfig = charonConfig;
        _recoveryConfig = recoveryConfig;
        _logger = logger;
        _serialPortManager = serialPortManager;
        _interOpWrapper = interOpWrapper;
        _otdrManager = otdrManager;
        _grpcSender = grpcSender;

        _id = rtuGeneralConfig.Value.RtuId;
    }

    private const string CharonIp = "192.168.88.101";
    private async Task<RtuInitializedDto> InitializeOtau(RtuInitializedDto result)
    {
        await Task.Delay(1);
        _mainCharon = new Charon(new NetAddress(CharonIp, 23), true, _charonConfig, _logger, _serialPortManager);
        var res = _mainCharon.InitializeOtauRecursively();
        if (res == _mainCharon.NetAddress)
            return new RtuInitializedDto(ReturnCode.OtauInitializationError);

        result.Serial = _mainCharon.Serial;
        result.OwnPortCount = _mainCharon.OwnPortCount;
        result.FullPortCount = _mainCharon.FullPortCount;
        result.Children = _mainCharon.GetChildrenDto();

        result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;

        _mainCharon.ShowOnDisplayMessageReady();

        return result;
    }

}