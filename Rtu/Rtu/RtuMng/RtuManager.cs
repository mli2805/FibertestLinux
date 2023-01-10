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
    private readonly IOptions<CharonConfig> _fullConfig;
    private readonly IWritableOptions<RecoveryConfig> _recoveryConfig;
    private readonly ILogger<RtuManager> _logger;
    private readonly SerialPortManager _serialPortManager;
    private readonly InterOpWrapper _interOpWrapper;
    private readonly OtdrManager _otdrManager;
    private readonly GrpcSender _grpcSender;

    private Guid _id;
    private Charon _mainCharon = null!;


    public RtuManager(IOptions<CharonConfig> fullConfig, IWritableOptions<RtuGeneralConfig> rtuGeneralConfig,
        IWritableOptions<MonitoringConfig> monitoringConfig, IWritableOptions<RecoveryConfig> recoveryConfig,
        ILogger<RtuManager> logger,
        SerialPortManager serialPortManager, InterOpWrapper interOpWrapper, OtdrManager otdrManager,
        GrpcSender grpcSender)
    {
        _rtuGeneralConfig = rtuGeneralConfig;
        _monitoringConfig = monitoringConfig;
        _fullConfig = fullConfig;
        _recoveryConfig = recoveryConfig;
        _logger = logger;
        _serialPortManager = serialPortManager;
        _interOpWrapper = interOpWrapper;
        _otdrManager = otdrManager;
        _grpcSender = grpcSender;

        _id = rtuGeneralConfig.Value.RtuId;
    }
}