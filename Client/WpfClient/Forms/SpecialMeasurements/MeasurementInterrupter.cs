using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class MeasurementInterrupter
    {
        private readonly ILogger _logger; 
        private readonly IWcfServiceCommonC2D _c2RWcfManager;

        public MeasurementInterrupter(ILogger logger, IWcfServiceCommonC2D c2RWcfManager)
        {
            _logger = logger;
            _c2RWcfManager = c2RWcfManager;
        }

        public async Task Interrupt(Rtu rtu, string log)
        {
            _logger.LogInfo(Logs.Client,$@"Interrupting {log}...");

            var dto = new InterruptMeasurementDto(rtu.Id, rtu.RtuMaker);
            await _c2RWcfManager.InterruptMeasurementAsync(dto);
        }
    }
}