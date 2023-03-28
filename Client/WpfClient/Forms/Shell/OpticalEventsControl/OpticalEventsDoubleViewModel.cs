using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class OpticalEventsDoubleViewModel : PropertyChangedBase
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        public OpticalEventsViewModel AllOpticalEventsViewModel { get; set; }
        public OpticalEventsViewModel ActualOpticalEventsViewModel { get; set; }

        public OpticalEventsDoubleViewModel(Model readModel, CurrentUser currentUser,
            OpticalEventsViewModel allOpticalEventsViewModel,
            OpticalEventsViewModel actualOpticalEventsViewModel)
        {
            _readModel = readModel;
            _currentUser = currentUser;
            ActualOpticalEventsViewModel = actualOpticalEventsViewModel;
            ActualOpticalEventsViewModel.TableTitle = Resources.SID_Current_accidents;
            AllOpticalEventsViewModel = allOpticalEventsViewModel;
            AllOpticalEventsViewModel.TableTitle = Resources.SID_All_optical_events;
        }

        public void AddMeasurement(MeasurementAdded measurementAdded)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == measurementAdded.TraceId);
            if (trace == null || !trace.ZoneIds.Contains(_currentUser.ZoneId) || !trace.IsAttached)
                return;

            var measurement = Mapper.Map<Measurement>(measurementAdded);
            AllOpticalEventsViewModel.AddEvent(measurement);

            ActualOpticalEventsViewModel.RemoveEventsOfTrace(measurement.TraceId);
            if (measurement.TraceState != FiberState.Ok)
                ActualOpticalEventsViewModel.AddEvent(measurement);
        }

        public void UpdateMeasurement(MeasurementUpdated evnt)
        {
            ActualOpticalEventsViewModel.UpdateEvent(evnt);
            AllOpticalEventsViewModel.UpdateEvent(evnt);
        }

        public void RenderMeasurementsFromSnapshot()
        {
            foreach (var measurement in _readModel.Measurements)
            {
                var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == measurement.TraceId);
                if (trace == null || !trace.ZoneIds.Contains(_currentUser.ZoneId) || !trace.IsAttached
                    || measurement.EventStatus <= EventStatus.JustMeasurementNotAnEvent)
                    continue;
                AllOpticalEventsViewModel.AddEvent(measurement);

                ActualOpticalEventsViewModel.RemoveEventsOfTrace(measurement.TraceId);
                if (measurement.TraceState != FiberState.Ok)
                    ActualOpticalEventsViewModel.AddEvent(measurement);
            }
        }

        public void AttachTrace(TraceAttached evnt)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == evnt.TraceId);
            if (trace == null || !trace.ZoneIds.Contains(_currentUser.ZoneId))
                return;

            var lastEventOnTrace = _readModel.Measurements.LastOrDefault(m =>
                m.TraceId == evnt.TraceId && m.EventStatus >= EventStatus.EventButNotAnAccident);
            if (lastEventOnTrace != null && lastEventOnTrace.TraceState != FiberState.Ok)
                ActualOpticalEventsViewModel.AddEvent(lastEventOnTrace);
        }

        public void DetachTrace(TraceDetached evnt)
        {
            //  ActualOpticalEventsViewModel.RemovePreviousEventForTraceIfExists(evnt.TraceId);
            ActualOpticalEventsViewModel.RemoveEventsOfTrace(evnt.TraceId);
        }

        public void DetachAllTraces(AllTracesDetached evnt)
        {
            foreach (var trace in _readModel.Traces.Where(t => t.RtuId == evnt.RtuId))
            {
                ActualOpticalEventsViewModel.RemoveEventsOfTrace(trace.TraceId);
            }
        }

        public void DetachOtau(OtauDetached evnt)
        {
            foreach (var traceId in evnt.TracesOnOtau)
            {
                ActualOpticalEventsViewModel.RemoveEventsOfTrace(traceId);
            }
        }

        public void CleanTrace(TraceCleaned evnt)
        {
            ActualOpticalEventsViewModel.RemoveEventsOfTrace(evnt.TraceId);
            AllOpticalEventsViewModel.RemoveEventsOfTrace(evnt.TraceId);
        }

        public void RemoveTrace(TraceRemoved evnt)
        {
            ActualOpticalEventsViewModel.RemoveEventsOfTrace(evnt.TraceId);
            AllOpticalEventsViewModel.RemoveEventsOfTrace(evnt.TraceId);
        }

        public void UpdateTrace(TraceUpdated evnt)
        {
            ActualOpticalEventsViewModel.RefreshRowsWithUpdatedTrace(evnt.Id);
            AllOpticalEventsViewModel.RefreshRowsWithUpdatedTrace(evnt.Id);
        }

        public void UpdateRtu(RtuUpdated evnt)
        {
            ActualOpticalEventsViewModel.RefreshRowsWithUpdatedRtu(evnt.RtuId);
            AllOpticalEventsViewModel.RefreshRowsWithUpdatedRtu(evnt.RtuId);
        }

        public void RemoveRtu(RtuRemoved evnt)
        {
            ActualOpticalEventsViewModel.RemoveEventsOfRtu(evnt.RtuId);
            AllOpticalEventsViewModel.RemoveEventsOfRtu(evnt.RtuId);
        }

        public void ChangeResponsibilities(ResponsibilitiesChanged evnt)
        {
            foreach (var pair in evnt.ResponsibilitiesDictionary)
            {
                var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == pair.Key);
                if (trace == null) continue; // not interested here in RTU
                if (!pair.Value.Contains(_currentUser.ZoneId)) continue; // for current zone this trace doesn't change

                if (trace.ZoneIds.Contains(_currentUser.ZoneId)) // was NOT became YES
                {
                    var lastMeasurementOnThisTrace = _readModel.Measurements.
                        LastOrDefault(m => m.TraceId == trace.TraceId && m.EventStatus > EventStatus.JustMeasurementNotAnEvent);
                    if (lastMeasurementOnThisTrace != null && lastMeasurementOnThisTrace.TraceState != FiberState.Ok)
                        ActualOpticalEventsViewModel.AddEvent(lastMeasurementOnThisTrace);

                    foreach (var measurement in _readModel.Measurements.
                        Where(m => m.TraceId == trace.TraceId && m.EventStatus > EventStatus.JustMeasurementNotAnEvent))
                        AllOpticalEventsViewModel.AddEvent(measurement);
                }
                else // was YES became NOT
                {
                    ActualOpticalEventsViewModel.RemoveEventsOfTrace(trace.TraceId);
                    AllOpticalEventsViewModel.RemoveEventsOfTrace(trace.TraceId);
                }
            }

        }


    }
}
