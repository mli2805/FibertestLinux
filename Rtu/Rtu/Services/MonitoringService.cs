using Fibertest.Utils;
using System.Diagnostics;

namespace Fibertest.Rtu
{
    public class MonitoringService : BackgroundService
    {
        private readonly ILogger<MonitoringService> _logger;
        private readonly IWritableOptions<MonitoringConfig> _config;
        private readonly RtuManager _rtuManager;

        public MonitoringService(ILogger<MonitoringService> logger, IWritableOptions<MonitoringConfig> config,
            RtuManager rtuManager)
        {
            _logger = logger;
            _config = config;
            _rtuManager = rtuManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"RTU monitoring service started. Process {pid}, thread {tid}");
            
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            await _rtuManager.InitializeRtu();
        }
    }
}
