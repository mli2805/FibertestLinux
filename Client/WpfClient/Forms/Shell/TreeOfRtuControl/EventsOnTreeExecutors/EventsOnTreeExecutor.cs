using System;
using Fibertest.Graph;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;
using AllTracesDetached = Fibertest.Graph.AllTracesDetached;
using BopNetworkEventAdded = Fibertest.Graph.BopNetworkEventAdded;
using NetworkEventAdded = Fibertest.Graph.NetworkEventAdded;
using OtauAttached = Fibertest.Graph.OtauAttached;
using OtauDetached = Fibertest.Graph.OtauDetached;
using RtuAtGpsLocationAdded = Fibertest.Graph.RtuAtGpsLocationAdded;
using RtuInitialized = Fibertest.Graph.RtuInitialized;
using RtuRemoved = Fibertest.Graph.RtuRemoved;
using RtuUpdated = Fibertest.Graph.RtuUpdated;
using TraceAdded = Fibertest.Graph.TraceAdded;
using TraceAttached = Fibertest.Graph.TraceAttached;
using TraceCleaned = Fibertest.Graph.TraceCleaned;
using TraceDetached = Fibertest.Graph.TraceDetached;
using TraceRemoved = Fibertest.Graph.TraceRemoved;
using TraceUpdated = Fibertest.Graph.TraceUpdated;

namespace Fibertest.WpfClient
{
    public class EventsOnTreeExecutor
    {
        private readonly ILogger _logger; 
        private readonly RtuEventsOnTreeExecutor _rtuEventsOnTreeExecutor;
        private readonly InitializeRtuEventOnTreeExecutor _initializeRtuEventOnTreeExecutor;
        private readonly TraceEventsOnTreeExecutor _traceEventsOnTreeExecutor;
        private readonly EchoEventsOnTreeExecutor _echoEventsOnTreeExecutor;
        private readonly ZoneEventsOnTreeExecutor _zoneEventsOnTreeExecutor;

        public EventsOnTreeExecutor(ILogger logger,
            RtuEventsOnTreeExecutor rtuEventsOnTreeExecutor, InitializeRtuEventOnTreeExecutor initializeRtuEventOnTreeExecutor,
            TraceEventsOnTreeExecutor traceEventsOnTreeExecutor, EchoEventsOnTreeExecutor echoEventsOnTreeExecutor,
            ZoneEventsOnTreeExecutor zoneEventsOnTreeExecutor)
        {
            _logger = logger;
            _rtuEventsOnTreeExecutor = rtuEventsOnTreeExecutor;
            _initializeRtuEventOnTreeExecutor = initializeRtuEventOnTreeExecutor;
            _traceEventsOnTreeExecutor = traceEventsOnTreeExecutor;
            _echoEventsOnTreeExecutor = echoEventsOnTreeExecutor;
            _zoneEventsOnTreeExecutor = zoneEventsOnTreeExecutor;
        }

        public void Apply(object e)
        {
            try
            {
                switch (e)
                {
                    case RtuAtGpsLocationAdded evnt: _rtuEventsOnTreeExecutor.AddRtuAtGpsLocation(evnt); return;
                    case RtuUpdated evnt: _rtuEventsOnTreeExecutor.UpdateRtu(evnt); return;
                    case RtuRemoved evnt: _rtuEventsOnTreeExecutor.RemoveRtu(evnt); return;
                    case OtauAttached evnt: _rtuEventsOnTreeExecutor.AttachOtau(evnt); return;
                    case OtauDetached evnt: _rtuEventsOnTreeExecutor.DetachOtau(evnt); return;
                    case AllTracesDetached evnt: _rtuEventsOnTreeExecutor.DetachAllTraces(evnt); return;
                    case NetworkEventAdded evnt: _rtuEventsOnTreeExecutor.AddNetworkEvent(evnt); return;
                    case BopNetworkEventAdded evnt: _rtuEventsOnTreeExecutor.AddBopNetworkEvent(evnt); return;

                    case RtuInitialized evnt: _initializeRtuEventOnTreeExecutor.InitializeRtu(evnt); return;

                    case TraceAdded evnt: _traceEventsOnTreeExecutor.AddTrace(evnt); return;
                    case TraceUpdated evnt: _traceEventsOnTreeExecutor.UpdateTrace(evnt); return;
                    case TraceCleaned evnt: _traceEventsOnTreeExecutor.CleanTrace(evnt); return;
                    case TraceRemoved evnt: _traceEventsOnTreeExecutor.RemoveTrace(evnt); return;
                    case TraceAttached evnt: _traceEventsOnTreeExecutor.AttachTrace(evnt); return;
                    case TraceDetached evnt: _traceEventsOnTreeExecutor.DetachTrace(evnt); return;
                    case MeasurementAdded evnt: _traceEventsOnTreeExecutor.AddMeasurement(evnt); return;

                    case TceWithRelationsAddedOrUpdated evnt: _traceEventsOnTreeExecutor.AddOrUpdateTceWithRelation(evnt); return;
                    case TceRemoved evnt: _traceEventsOnTreeExecutor.RemoveTce(evnt); return;

                    case BaseRefAssigned evnt: _echoEventsOnTreeExecutor.AssignBaseRef(evnt); return;
                    case MonitoringSettingsChanged evnt: _echoEventsOnTreeExecutor.ChangeMonitoringSettings(evnt); return;
                    case MonitoringStarted evnt: _echoEventsOnTreeExecutor.StartMonitoring(evnt); return;
                    case MonitoringStopped evnt: _echoEventsOnTreeExecutor.StopMonitoring(evnt); return;

                    case ResponsibilitiesChanged evnt: _zoneEventsOnTreeExecutor.ChangeResponsibility(evnt); return;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(Logs.Client,exception.Message);
                _logger.LogError(Logs.Client,$@"EventsOnTreeExecutor crashed while applying event {e.GetType().FullName}");
                throw;
            }
        }
    }
}