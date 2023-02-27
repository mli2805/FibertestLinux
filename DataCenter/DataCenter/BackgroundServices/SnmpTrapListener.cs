using System.Diagnostics;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public class SnmpTrapListener : BackgroundService
    {
        private readonly ILogger<SnmpTrapListener> _logger;
        private readonly SnmpEngine2 _snmpEngine;

        public SnmpTrapListener(ILogger<SnmpTrapListener> logger, SnmpEngine2 snmpEngine)
        {
            _logger = logger;
            _snmpEngine = snmpEngine;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logger.Info(Logs.DataCenter, $"SNMP trap listener starts. Process {pid}, thread {tid}");
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            await _snmpEngine.Start(stoppingToken);
        }
    }
}
