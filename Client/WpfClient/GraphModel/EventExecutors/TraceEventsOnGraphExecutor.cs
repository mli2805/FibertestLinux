using System;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class TraceEventsOnGraphExecutor
    {
        private readonly GraphReadModel _graphModel;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly AccidentEventsOnGraphExecutor _accidentEventsOnGraphExecutor;

        public TraceEventsOnGraphExecutor(GraphReadModel graphModel, Model readModel, 
            CurrentUser currentUser, AccidentEventsOnGraphExecutor accidentEventsOnGraphExecutor)
        {
            _graphModel = graphModel;
            _readModel = readModel;
            _currentUser = currentUser;
            _accidentEventsOnGraphExecutor = accidentEventsOnGraphExecutor;
        }

        public void AddTrace(TraceAdded evnt)
        {
            _graphModel.SetFutureTraceLightOnOff(evnt.TraceId, evnt.FiberIds, false);
            _graphModel.ChangeFutureTraceColor(evnt.TraceId, evnt.FiberIds, FiberState.NotJoined);
        }

        // event applied to ReadModel firstly and at this moment trace could be cleaned/removed, so fibers list should be prepared beforehand
        // but in case trace was hidden check fiberVm/nodeVm on null before operations
        public void CleanTrace(TraceCleaned evnt)
        {
            _graphModel.ExtinguishAll();
            foreach (var fiberId in evnt.FiberIds)
            {
                var fiberVm = _graphModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                fiberVm?.RemoveState(evnt.TraceId);
            }
        }

        // event applied to ReadModel firstly and at this moment trace could be cleaned/removed, so fibers list should be prepared beforehand
        // but in case trace was hidden check fiberVm/nodeVm on null before operations
        public void RemoveTrace(TraceRemoved evnt)
        {
            _graphModel.ExtinguishAll();
            foreach (var fiberId in evnt.FiberIds)
            {
                var fiberVm = _graphModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                if (fiberVm == null) continue;
                fiberVm.RemoveState(evnt.TraceId);
                if (fiberVm.State == FiberState.NotInTrace)
                    _graphModel.Data.Fibers.Remove(fiberVm);
            }
            foreach (var nodeId in evnt.NodeIds)
            {
                if (_graphModel.Data.Fibers.Any(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId))
                    continue;
                var nodeVm = _graphModel.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
                if (nodeVm?.Type != EquipmentType.Rtu)
                    _graphModel.Data.Nodes.Remove(nodeVm);
            }
        }

        public void AttachTrace(TraceAttached evnt)
        {
            if (!ShouldAcceptEventForTrace(evnt.TraceId)) return;

            _accidentEventsOnGraphExecutor.ShowMonitoringResult(new MeasurementAdded()
            {
                TraceId = evnt.TraceId,
                TraceState = evnt.PreviousTraceState,
                Accidents = evnt.AccidentsInLastMeasurement,
            });
        }

        public void DetachTrace(TraceDetached evnt)
        {
            var trace = _readModel.Traces.First(t => t.TraceId == evnt.TraceId);
            DetachTrace(trace);
        }

        public void DetachTrace(Trace trace)
        {
            if (!ShouldAcceptEventForTrace(trace.TraceId)) return;

            foreach (var fiberId in trace.FiberIds)
            {
                var fiberVm = _graphModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                if (fiberVm != null)
                    fiberVm.SetState(trace.TraceId, trace.State);
            }
            _graphModel.CleanAccidentPlacesOnTrace(trace.TraceId);
        }

        private bool ShouldAcceptEventForTrace(Guid traceId)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Traces.First(t => t.TraceId == traceId).ZoneIds.Contains(_currentUser.ZoneId)) return false;

            return true;
        }
    }
}