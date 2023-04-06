using AutoMapper;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public partial class EventStoreService
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingWebApiProfile>()).CreateMapper();

        private async Task NotifyWebClient(object cmd)
        {
            switch (cmd)
            {
                case UpdateMeasurement _:
                {
                    var evnt = Mapper.Map<UpdateMeasurementDto>(cmd);
                    await _ftSignalRClient.NotifyAll("UpdateMeasurement", evnt.ToCamelCaseJson());
                    break;
                }

                case AddRtuAtGpsLocation _:
                case RemoveRtu _:
                case DetachAllTraces _:
                case CleanTrace _:
                case RemoveTrace _:
                case AddTrace _:
                {
                    await _ftSignalRClient.NotifyAll("FetchTree", null);
                    break;
                }
                case AttachTrace dto:
                {
                    await OnTraceTached(dto.TraceId, true);
                    break;
                }
                case DetachTrace dto:
                {
                    await OnTraceTached(dto.TraceId, false);
                    break;
                }
            }
        }

        private async Task OnTraceTached(Guid traceId, bool isAttach)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null) return;
            var meas = _writeModel.Measurements.LastOrDefault(m => m.TraceId == traceId);
            var signal = new TraceTachDto() { TraceId = traceId, Attach = isAttach, TraceState = trace.State, SorFileId = meas?.SorFileId ?? -1 };

            await _ftSignalRClient.NotifyAll("TraceTach", signal.ToCamelCaseJson());
        }


    }
}
