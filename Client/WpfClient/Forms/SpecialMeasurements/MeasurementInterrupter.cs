using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class MeasurementInterrupter
    {
        private readonly ILogger _logger;
        private readonly GrpcC2RService _grpcC2RService;

        public MeasurementInterrupter(ILogger logger, GrpcC2RService grpcC2RService)
        {
            _logger = logger;
            _grpcC2RService = grpcC2RService;
        }

        public async Task Interrupt(Rtu rtu, string log)
        {
            _logger.Info(Logs.Client,$@"Interrupting {log}...");

            var dto = new InterruptMeasurementDto(rtu.Id, rtu.RtuMaker);
            await _grpcC2RService.SendAnyC2RRequest<InterruptMeasurementDto, RequestAnswer>(dto);
        }
    }
}