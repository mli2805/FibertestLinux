using System;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;
using GMap.NET;

namespace Fibertest.WpfClient
{
    public class AccidentEventsOnGraphExecutor
    {
        private readonly GraphReadModel _graphReadModel;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        public AccidentEventsOnGraphExecutor(GraphReadModel graphReadModel, Model readModel,
            CurrentUser currentUser)
        {
            _graphReadModel = graphReadModel;
            _readModel = readModel;
            _currentUser = currentUser;
        }

        public void ShowMonitoringResult(MeasurementAdded evnt)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == evnt.TraceId);
            if (trace == null || _currentUser.ZoneId != Guid.Empty &&
                                    !trace.ZoneIds.Contains(_currentUser.ZoneId)) return;

            _graphReadModel.ChangeTraceColor(evnt.TraceId, evnt.TraceState);
            _graphReadModel.CleanAccidentPlacesOnTrace(evnt.TraceId); // accidents on trace could change, so old should be cleaned and new drawn
            if (evnt.TraceState != FiberState.Unknown && evnt.TraceState != FiberState.Ok && evnt.TraceState != FiberState.NoFiber)
                evnt.Accidents.ForEach(a => ShowAccidentPlaceOnTrace(a, evnt.TraceId));
        }

        private void ShowAccidentPlaceOnTrace(AccidentOnTraceV2 accident, Guid traceId)
        {
            if (accident.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                ShowBadSegment(accident, traceId);
            else
                ShowPoint(accident, traceId);
        }

        private void ShowPoint(AccidentOnTraceV2 accidentInPoint, Guid traceId)
        {
            AddAccidentNode(accidentInPoint.AccidentCoors, traceId, accidentInPoint.AccidentSeriousness);
        }

        private void AddAccidentNode(PointLatLng accidentGps, Guid traceId, FiberState state)
        {
            var accidentNode = new NodeVm()
            {
                Id = Guid.NewGuid(),
                Position = accidentGps,
                Type = EquipmentType.AccidentPlace,
                AccidentOnTraceVmId = traceId,
                State = state,
            };
            _graphReadModel.Data.Nodes.Add(accidentNode);
        }

        private void ShowBadSegment(AccidentOnTraceV2 accidentInOldEvent, Guid traceId)
        {
            var fibers = _readModel.GetTraceFibersBetweenLandmarks(traceId, accidentInOldEvent.Left.LandmarkIndex,
                accidentInOldEvent.Right.LandmarkIndex);

            foreach (var fiberId in fibers)
            {
                var fiberVm = _graphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                if (fiberVm != null)
                    fiberVm.SetBadSegment(traceId, accidentInOldEvent.AccidentSeriousness);
            }
        }

    }
}