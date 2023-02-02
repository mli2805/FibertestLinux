using System.Collections.Concurrent;
using Fibertest.CharonLib;
using Fibertest.Dto;
using Fibertest.Utils;
using Newtonsoft.Json;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

    private readonly IWritableConfig<RtuConfig> _config;
    private readonly ILogger<RtuManager> _logger;
    private readonly SerialPortManager _serialPortManager;
    private readonly InterOpWrapper _interOpWrapper;
    private readonly OtdrManager _otdrManager;
    private readonly GrpcSender _grpcSender;

    private Guid _id;
    private Charon _mainCharon = null!;
    private int _measurementNumber;
    private TimeSpan _preciseMakeTimespan;
    private TimeSpan _preciseSaveTimespan;
    private TimeSpan _fastSaveTimespan;

    private CancellationTokenSource? _cancellationTokenSource;
    private MonitoringQueue _monitoringQueue;
    public readonly ConcurrentQueue<object> ShouldSendHeartbeat = new ConcurrentQueue<object>();

    public RtuManager(IWritableConfig<RtuConfig> config, 
        ILogger<RtuManager> logger, MonitoringQueue monitoringQueue,
        SerialPortManager serialPortManager, InterOpWrapper interOpWrapper, OtdrManager otdrManager,
        GrpcSender grpcSender)
    {
        _config = config;
        _logger = logger;
        _monitoringQueue = monitoringQueue;
        _serialPortManager = serialPortManager;
        _interOpWrapper = interOpWrapper;
        _otdrManager = otdrManager;
        _grpcSender = grpcSender;

        _id = config.Value.General.RtuId;
    }
}