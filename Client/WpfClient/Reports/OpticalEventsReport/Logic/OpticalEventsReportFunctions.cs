using System;
using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public static class OpticalEventsReportFunctions
    {
        public static List<List<string>> Create(List<OpticalEventModel> events, OpticalEventsReportModel reportModel)
        {
            var result = new List<List<string>>();
            var data = Calculate(events, reportModel);

            foreach (var eventStatus in EventStatusExt.EventStatusesInRightOrder)
            {
                if (data.ContainsKey(eventStatus))
                    result.Add(Convert(eventStatus, data[eventStatus], reportModel));
            }
            return result;
        }

        private static List<string> Convert(EventStatus eventStatus, 
             Dictionary<FiberState, int> values, OpticalEventsReportModel reportModel)
        {
            var statusLine = new List<string>() { eventStatus.GetLocalizedString() };
            foreach (var state in reportModel.TraceStateSelectionViewModel.GetCheckedStates())
            {
                statusLine.Add(values.ContainsKey(state) ? values[state].ToString() : @"0");
            }
            return statusLine;
        }

        private static Dictionary<EventStatus, Dictionary<FiberState, int>>
            Calculate(List<OpticalEventModel> events, OpticalEventsReportModel reportModel)
        {
            var result = new Dictionary<EventStatus, Dictionary<FiberState, int>>();
            foreach (var meas in events.Where(r => IsEventForReport(r, reportModel)))
            {
                if (result.ContainsKey(meas.EventStatus))
                {
                    if (result[meas.EventStatus].ContainsKey(meas.TraceState))
                    {
                        result[meas.EventStatus][meas.TraceState]++;
                    }
                    else
                    {
                        result[meas.EventStatus].Add(meas.TraceState, 1);
                    }
                }
                else
                {
                    result.Add(meas.EventStatus, new Dictionary<FiberState, int> { { meas.TraceState, 1 } });
                }
            }
            return result;
        }

        private static bool IsEventForReport(this OpticalEventModel opticalEventModel, OpticalEventsReportModel reportModel)
        {
            if (opticalEventModel.MeasurementTimestamp.Date < reportModel.DateFrom.Date) return false;
            if (opticalEventModel.MeasurementTimestamp.Date > reportModel.DateTo.Date) return false;
            if (!reportModel.EventStatusViewModel.GetCheckedStatuses().Contains(opticalEventModel.EventStatus)) return false;
            return reportModel.TraceStateSelectionViewModel.GetCheckedStates().Contains(opticalEventModel.TraceState);
        }

        /// <summary>
        /// Find closest OK event which happened after every Accident in events
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public static Dictionary<int, DateTime> GetAccidentsClosingTimes(this List<OpticalEventModel> events)
        {
            var allAccidents = events.Where(r => r.EventStatus > EventStatus.EventButNotAnAccident).ToList();
            var result = new Dictionary<int, DateTime>();
            foreach (var opticalEventModel in allAccidents)
            {
                var okEvent = events.FirstOrDefault(e =>
                    e.TraceId == opticalEventModel.TraceId && e.TraceState == FiberState.Ok
                                                           && e.MeasurementTimestamp >= opticalEventModel.MeasurementTimestamp);
                if (okEvent != null)
                    result.Add(opticalEventModel.SorFileId, okEvent.MeasurementTimestamp);
            }
            return result;
        }
    }
}