using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter
{
    public static class EventsExt
    {
        public static bool FilterRtu(this Graph.Rtu rtu, User user, string? filterRtu)
        {
            if (!rtu.ZoneIds.Contains(user.ZoneId)) return false;
            if (!string.IsNullOrEmpty(filterRtu) && !rtu.Title.Contains(filterRtu)) return false;
            return true;
        }

        public static bool FilterRtuWithProblems(this Graph.Rtu rtu, User user, string? filterRtu)
        {
            if (!rtu.ZoneIds.Contains(user.ZoneId)) return false;
            if (rtu.IsAllRight) return false;
            if (!string.IsNullOrEmpty(filterRtu) && !rtu.Title.Contains(filterRtu)) return false;
            return true;
        }

        public static bool Filter(this NetworkEvent networkEvent, string filterRtu, Model writeModel,
            User user)
        {
            var rtu = writeModel.Rtus.FirstOrDefault(r => r.Id == networkEvent.RtuId);
            if (rtu == null
                || !rtu.ZoneIds.Contains(user.ZoneId)
                || (!string.IsNullOrEmpty(filterRtu) && !rtu.Title.Contains(filterRtu)))
            {
                return false;
            }

            return true;
        }

        public static bool Filter(this BopNetworkEvent bopNetworkEvent, string filterRtu, Model writeModel,
            User user)
        {
            var rtu = writeModel.Rtus.FirstOrDefault(r => r.Id == bopNetworkEvent.RtuId);
            if (rtu == null
                || !rtu.ZoneIds.Contains(user.ZoneId)
                || (!string.IsNullOrEmpty(filterRtu) && !rtu.Title.Contains(filterRtu)))
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<NetworkEvent> Sort(this IEnumerable<NetworkEvent> input, string param)
        {
            return param == "asc" ? input.OrderBy(o => o.Ordinal) : input.OrderByDescending(o => o.Ordinal);
        }

        public static IEnumerable<BopNetworkEvent> Sort(this IEnumerable<BopNetworkEvent> input, string param)
        {
            return param == "asc" ? input.OrderBy(o => o.Ordinal) : input.OrderByDescending(o => o.Ordinal);
        }

        public static bool Filter(this Measurement measurement, string filterRtu, string filterTrace, Model writeModel,
            User user)
        {
            if (measurement.EventStatus == EventStatus.JustMeasurementNotAnEvent)
                return false;

            var rtu = writeModel.Rtus.FirstOrDefault(r => r.Id == measurement.RtuId);
            if (rtu == null
                || !rtu.ZoneIds.Contains(user.ZoneId)
                || (!string.IsNullOrEmpty(filterRtu) && !rtu.Title.Contains(filterRtu)))
            {
                return false;
            }

            var trace = writeModel.Traces.FirstOrDefault(t => t.TraceId == measurement.TraceId);
            if (trace == null
                || !trace.ZoneIds.Contains(user.ZoneId)
                || (!string.IsNullOrEmpty(filterTrace) && !trace.Title.Contains(filterTrace)))
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<Measurement> Sort(this IEnumerable<Measurement> input, string param)
        {
            return param == "asc" ? input.OrderBy(o => o.SorFileId) : input.OrderByDescending(o => o.SorFileId);
        }
    }
}
