using System.Collections.Concurrent;
using Fibertest.CharonLib;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class RtuManager
{

    private readonly IWritableConfig<RtuConfig> _config;
    private readonly ILogger<RtuManager> _logger;
    private readonly SerialPortManager _serialPortManager;
    private readonly InterOpWrapper _interOpWrapper;
    private readonly OtdrManager _otdrManager;
    private readonly GrpcR2DService _grpcR2DService;

    private Guid _id;
    private Charon _mainCharon = null!;
    private int _measurementNumber;
    private TimeSpan _preciseMakeTimespan;
    private TimeSpan _preciseSaveTimespan;
    private TimeSpan _fastSaveTimespan;

    private TreeOfAcceptableMeasParams? _treeOfAcceptableMeasParams;
    public CancellationToken RtuServiceCancellationToken;
    private CancellationTokenSource? _rtuManagerCts;
    private MonitoringQueue _monitoringQueue;
    public readonly ConcurrentQueue<object> ShouldSendHeartbeat = new ConcurrentQueue<object>();

    private bool _wasMonitoringOn;

   
    private readonly object _lastSuccessfulMeasTimestampLocker = new object();
    private DateTime _lastSuccessfulMeasTimestamp;
    public DateTime LastSuccessfulMeasTimestamp
    {
        get { lock (_lastSuccessfulMeasTimestampLocker) { return _lastSuccessfulMeasTimestamp; } }
        set { lock (_lastSuccessfulMeasTimestampLocker) { _lastSuccessfulMeasTimestamp = value; } }
    }

    private readonly object _isRtuInitializedLocker = new object();
    private bool _isRtuInitialized;
    public bool IsRtuInitialized
    {
        get { lock (_isRtuInitializedLocker) { return _isRtuInitialized; } }
        set { lock (_isRtuInitializedLocker) { _isRtuInitialized = value; } }
    }

    public RtuManager(IWritableConfig<RtuConfig> config,
        ILogger<RtuManager> logger, MonitoringQueue monitoringQueue,
        InterOpWrapper interOpWrapper, OtdrManager otdrManager,
        GrpcR2DService grpcR2DService)
    {
        IsRtuInitialized = false;
        _config = config;
        _logger = logger;
        _monitoringQueue = monitoringQueue;
        _serialPortManager = new SerialPortManager();
        _serialPortManager.Initialize(_config.Value.Charon, logger);
        _interOpWrapper = interOpWrapper;
        _otdrManager = otdrManager;
        _grpcR2DService = grpcR2DService;

        _id = config.Value.General.RtuId;
    }
}