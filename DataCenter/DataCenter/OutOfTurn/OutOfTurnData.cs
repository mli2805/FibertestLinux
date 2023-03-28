using System.Collections.Concurrent;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter
{
    public class OutOfTurnRequest
    {
        public DoOutOfTurnPreciseMeasurementDto Dto;
        public DateTime Timestamp; // Time when request placed into queue

        public OutOfTurnRequest(DoOutOfTurnPreciseMeasurementDto dto, DateTime timestamp)
        {
            Dto = dto;
            Timestamp = timestamp;
        }
    }

    public class OutOfTurnData
    {
        // RTU Id - < Trace Id - Request >
        public readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, OutOfTurnRequest>> Requests =
            new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, OutOfTurnRequest>>();

        public void AddNewRequest(DoOutOfTurnPreciseMeasurementDto dto)
        {
            var newDict = new ConcurrentDictionary<Guid, OutOfTurnRequest>();
            newDict.TryAdd(dto.PortWithTraceDto!.TraceId, 
                new OutOfTurnRequest(dto, DateTime.Now)); // no problem could be with empty dict

            Requests.AddOrUpdate(dto.RtuId, newDict,
                (_, oneRtuDict) =>
                {
                    oneRtuDict.AddOrUpdate(
                        // if there is no request for this trace/port - add it
                        dto.PortWithTraceDto.TraceId, new OutOfTurnRequest(dto, DateTime.Now),
                        // else - change time
                        (_, request) => { request.Timestamp = DateTime.Now; return request; });
                    return oneRtuDict;
                });

            // logFile.AppendLine($"Request added or updated, Queue of RTU {dto.RtuId.First6()} contains {Requests[dto.RtuId].Count} requests");
        }

        public DoOutOfTurnPreciseMeasurementDto? GetNextRequest(RtuOccupations rtuOccupations, string trapSenderUser)
        {
            // local copy
            var requests = Requests.ToArray();

            foreach (var oneRtuDict in requests)
            {
                if (oneRtuDict.Value.IsEmpty)
                    continue;
                
                if (!rtuOccupations.TrySetOccupation(oneRtuDict.Key, RtuOccupation.DoPreciseMeasurementOutOfTurn,
                                      trapSenderUser, out RtuOccupationState? _))
                {
                    // logFile.AppendLine($"RTU {oneRtuDict.Key.First6()} is busy, {oneRtuDict.Value.Count} requests in queue");
                    continue;
                }

                var oneRtuRequests = oneRtuDict.Value.Values.OrderBy(r => r.Timestamp);
                var dto = oneRtuRequests.First().Dto;

                Requests[oneRtuDict.Key]
                    .TryRemove(dto.PortWithTraceDto!.TraceId, out OutOfTurnRequest? _);
                // logFile.AppendLine($"Sending request for RTU {dto.RtuId.First6()} / Trace {dto.PortWithTraceDto.TraceId.First6()}.");
                // logFile.AppendLine($"  Now queue of RTU {dto.RtuId.First6()} contains {Requests[dto.RtuId].Count} requests");

                return dto;
            }

            return null;
        }
    }
}
