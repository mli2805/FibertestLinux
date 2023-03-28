using System;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;
using GMap.NET;

namespace Fibertest.WpfClient
{
    public class RtuEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly NodeEventsOnGraphExecutor _nodeEventsOnGraphExecutor;
        private readonly TraceEventsOnGraphExecutor _traceEventsOnGraphExecutor;

        public RtuEventsOnGraphExecutor(GraphReadModel model, Model readModel, CurrentUser currentUser,
            NodeEventsOnGraphExecutor nodeEventsOnGraphExecutor, TraceEventsOnGraphExecutor traceEventsOnGraphExecutor)
        {
            _model = model;
            _readModel = readModel;
            _currentUser = currentUser;
            _nodeEventsOnGraphExecutor = nodeEventsOnGraphExecutor;
            _traceEventsOnGraphExecutor = traceEventsOnGraphExecutor;
        }

        public void AddRtuAtGpsLocation(RtuAtGpsLocationAdded evnt)
        {
            if (_currentUser.ZoneId != Guid.Empty) return;

            var nodeVm = new NodeVm()
            {
                Id = evnt.NodeId,
                State = FiberState.Ok,
                Type = EquipmentType.Rtu,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude),
                Title = evnt.Title,
            };
            _model.Data.Nodes.Add(nodeVm);
        }

        public void UpdateRtu(RtuUpdated evnt)
        {
            var rtu = _readModel.Rtus.First(r => r.Id == evnt.RtuId);

            if (_currentUser.ZoneId != Guid.Empty &&
                !rtu.ZoneIds.Contains(_currentUser.ZoneId)) return;

            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == rtu.NodeId);
            if (nodeVm == null) return;
            nodeVm.Title = evnt.Title;
            nodeVm.Position = evnt.Position;
        }

        public void RemoveRtu(RtuRemoved evnt)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                _model.Data.Nodes.All(n => n.Id != evnt.RtuNodeId)) return;

            foreach (var pair in evnt.FibersFromCleanedTraces)
            {
                _model.Data.Fibers.FirstOrDefault(f => f.Id == pair.Key)?.RemoveState(pair.Value);
            }
            _nodeEventsOnGraphExecutor.RemoveNodeWithAllHisFibersUptoRealNode(evnt.RtuNodeId);
        }

        public void DetachOtau(OtauDetached evnt)
        {
            foreach (var traceId in evnt.TracesOnOtau)
            {
                var trace = _readModel.Traces.First(t => t.TraceId == traceId);
                _traceEventsOnGraphExecutor.DetachTrace(trace);
            }
        }

        public void DetachAllTraces(AllTracesDetached evnt)
        {
            foreach (var trace in _readModel.Traces.Where(t => t.RtuId == evnt.RtuId))
            {
                _traceEventsOnGraphExecutor.DetachTrace(trace);
            }
        }
    }
}