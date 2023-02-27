using System;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class OpticalEventsExecutor
    {
        private readonly ILogger _logger; 
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly SystemState _systemState;

        public OpticalEventsExecutor(ILogger logger, OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, SystemState systemState)
        {
            _logger = logger;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _systemState = systemState;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case MeasurementAdded evnt:
                    if (evnt.EventStatus > EventStatus.JustMeasurementNotAnEvent)
                        _opticalEventsDoubleViewModel.AddMeasurement(evnt);
                    break;
                case MeasurementUpdated evnt:
                    _opticalEventsDoubleViewModel.UpdateMeasurement(evnt); break;

                case TraceAttached evnt:
                    _opticalEventsDoubleViewModel.AttachTrace(evnt); break;
                case TraceDetached evnt:
                    _opticalEventsDoubleViewModel.DetachTrace(evnt); break;
                case AllTracesDetached evnt:
                    _opticalEventsDoubleViewModel.DetachAllTraces(evnt); break;
                case OtauDetached evnt: 
                    _opticalEventsDoubleViewModel.DetachOtau(evnt); break;

                case TraceRemoved evnt: _opticalEventsDoubleViewModel.RemoveTrace(evnt); break;
                case TraceUpdated evnt: _opticalEventsDoubleViewModel.UpdateTrace(evnt); break;
                case TraceCleaned evnt: _opticalEventsDoubleViewModel.CleanTrace(evnt); break;

                case RtuRemoved evnt: _opticalEventsDoubleViewModel.RemoveRtu(evnt); break;
                case RtuUpdated evnt: _opticalEventsDoubleViewModel.UpdateRtu(evnt); break;

                case ResponsibilitiesChanged evnt: _opticalEventsDoubleViewModel.ChangeResponsibilities(evnt); break;
                case EventsAndSorsRemoved evnt: _opticalEventsDoubleViewModel.
                    AllOpticalEventsViewModel.RemoveEventsAndSors(evnt); break;
            }

            try
            {
                _systemState.HasActualOpticalProblems =
                    _opticalEventsDoubleViewModel.ActualOpticalEventsViewModel.Rows.Any();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                _logger.Error(Logs.Client,exception.Message);
            }

        }
    }
}