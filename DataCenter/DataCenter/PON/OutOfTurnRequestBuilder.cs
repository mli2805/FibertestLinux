using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public class OutOfTurnRequestBuilder
    {
        private readonly ILogger<OutOfTurnRequestBuilder> _logger;
        private readonly Model _writeModel;

        public OutOfTurnRequestBuilder(ILogger<OutOfTurnRequestBuilder> logger, Model writeModel)
        {
            _logger = logger;
            _writeModel = writeModel;
        }

        public DoOutOfTurnPreciseMeasurementDto? BuildDto(TrapParserResult parsedTrap)
        {
            var relation = FindRelation(parsedTrap);
            if (relation == null) return null;
            var trace = GetTrace(relation);
            return trace == null ? null : BuildOutOfTurnDto(relation, trace);
        }

        private GponPortRelation? FindRelation(TrapParserResult res)
        {
            var relation = _writeModel.GponPortRelations.FirstOrDefault(r => r.TceId == res.TceId
                                                                             && r.SlotPosition == res.Slot
                                                                             && r.GponInterface == res.GponInterface);
            if (relation == null)
                _logger.LogInfo(Logs.SnmpTraps, $"There is no relation for gpon interface {res.GponInterface}");
          
            return relation;
        }

        private Trace? GetTrace(GponPortRelation relation)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == relation.RtuId);
            if (rtu == null || rtu.MonitoringState != MonitoringState.On)
            {
                _logger.LogInfo(Logs.SnmpTraps, "RTU is in Manual state or not found.");
                return null;
            }

            if (!_writeModel.TryGetTrace(relation.TraceId, out Trace? trace))
            {
                _logger.LogInfo(Logs.SnmpTraps, $"There is no trace on gpon interface {relation.GponInterface}");
                return null;
            }

            if (!trace!.IsIncludedInMonitoringCycle)
            {
                _logger.LogInfo(Logs.SnmpTraps, "Trace excluded from monitoring cycle");
                return null;
            }

            return trace;
        }

        private DoOutOfTurnPreciseMeasurementDto BuildOutOfTurnDto(GponPortRelation relation, Trace trace)
        {
             var dto = new DoOutOfTurnPreciseMeasurementDto(relation.RtuId, relation.RtuMaker)
            {
                Id = Guid.NewGuid(),
                PortWithTraceDto = new PortWithTraceDto(relation.OtauPortDto!, trace.TraceId),
                IsTrapCaused = true,
            };

            _logger.LogInfo(Logs.SnmpTraps, $"Request for trace {trace.Title} created.");
            return dto;
        }


    }
}
