using System;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public enum EventAcceptability
    {
        Full, // trace is included in current user's zone 
        Partly, // RTU is included but trace is not
        No, // neither RTU nor trace are included 
    }
    public class TraceEventsOnTreeExecutor
    {
        private readonly ILifetimeScope _globalScope;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly ILogger _logger; 
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;

        public TraceEventsOnTreeExecutor(ILifetimeScope globalScope, TreeOfRtuModel treeOfRtuModel, 
            ILogger logger, CurrentUser currentUser, Model readModel)
        {
            _globalScope = globalScope;
            _treeOfRtuModel = treeOfRtuModel;
            _logger = logger;
            _currentUser = currentUser;
            _readModel = readModel;
        }

        private EventAcceptability ShouldAcceptEventForTrace(Guid traceId)
        {
            if (_currentUser.ZoneId == Guid.Empty) return EventAcceptability.Full;

            var trace = _readModel.Traces.First(t => t.TraceId == traceId);

            if (!_readModel.Rtus.First(r => r.Id == trace.RtuId).ZoneIds.Contains(_currentUser.ZoneId))
                return EventAcceptability.No;

            return trace.ZoneIds.Contains(_currentUser.ZoneId) ? EventAcceptability.Full : EventAcceptability.Partly;
        }

        public void AddTrace(TraceAdded e)
        {
            var acceptable = ShouldAcceptEventForTrace(e.TraceId);
            if (acceptable == EventAcceptability.No) return;

            var rtuLeaf = (RtuLeaf?)_treeOfRtuModel.GetById(e.RtuId);
            if (rtuLeaf == null) return;
            var traceLeaf = _globalScope.Resolve<TraceLeaf>(new NamedParameter(@"parent", rtuLeaf));

            traceLeaf.Id = e.TraceId;
            traceLeaf.Title = e.Title;
            traceLeaf.TraceState = FiberState.NotJoined;
            traceLeaf.IsInZone = acceptable == EventAcceptability.Full;
            traceLeaf.Color = acceptable == EventAcceptability.Full ? Brushes.Blue : Brushes.LightGray;

            rtuLeaf.ChildrenImpresario.Children.Add(traceLeaf);
            rtuLeaf.IsExpanded = true;
        }

        public void UpdateTrace(TraceUpdated e)
        {
            if (ShouldAcceptEventForTrace(e.Id) == EventAcceptability.No) return;

            var traceLeaf = _treeOfRtuModel.GetById(e.Id);
            if (traceLeaf == null) return;
            traceLeaf.Title = e.Title;
        }

        public void CleanTrace(TraceCleaned e)
        {
            var traceLeaf = _treeOfRtuModel.GetById(e.TraceId);
            if (traceLeaf == null) return;

            var rtuLeaf = traceLeaf.Parent is RtuLeaf ? (RtuLeaf)traceLeaf.Parent : (RtuLeaf)traceLeaf.Parent.Parent;
            rtuLeaf.ChildrenImpresario.Children.Remove(traceLeaf);
        }

        public void RemoveTrace(TraceRemoved e)
        {
            var traceLeaf = _treeOfRtuModel.GetById(e.TraceId);
            if (traceLeaf == null) return;

            var rtuLeaf = traceLeaf.Parent is RtuLeaf ? (RtuLeaf)traceLeaf.Parent : (RtuLeaf)traceLeaf.Parent.Parent;
            rtuLeaf.ChildrenImpresario.Children.Remove(traceLeaf);
        }

        public void AttachTrace(TraceAttached e)
        {
            var acceptable = ShouldAcceptEventForTrace(e.TraceId);
            if (acceptable == EventAcceptability.No) return;

            var traceLeaf = (TraceLeaf?)_treeOfRtuModel.GetById(e.TraceId);
            if (traceLeaf == null) return;
            var rtuLeaf = (RtuLeaf?)_treeOfRtuModel.GetById(traceLeaf.Parent.Id);
            if (rtuLeaf == null) return;
            var portOwner = rtuLeaf.GetPortOwner(e.OtauPortDto);
            if (portOwner == null) return;

            var port = e.OtauPortDto.OpticalPort;

            var newTraceLeaf = _globalScope.Resolve<TraceLeaf>(new NamedParameter(@"parent", portOwner));
            newTraceLeaf.Id = e.TraceId;
            newTraceLeaf.TraceState = e.PreviousTraceState;
            newTraceLeaf.Title = traceLeaf.Title;
            newTraceLeaf.IsInZone = acceptable == EventAcceptability.Full;
            newTraceLeaf.TraceToTceLinkState = traceLeaf.TraceToTceLinkState;
          //  newTraceLeaf.Color = acceptable == EventAcceptability.Full ? Brushes.Black : Brushes.LightGray;
            newTraceLeaf.PortNumber = port;

            if (port < 1 || port > portOwner.ChildrenImpresario.Children.Count)
            {
                _logger.Info(Logs.Client,$@"Trace {traceLeaf.Title}, port = {port} is wrong");
            }
            portOwner.ChildrenImpresario.Children[port - 1] = newTraceLeaf;
            newTraceLeaf.BaseRefsSet = traceLeaf.BaseRefsSet;
            rtuLeaf.ChildrenImpresario.Children.Remove(traceLeaf);
        }

        public void DetachTrace(TraceDetached e)
        {
            DetachTrace(e.TraceId);
        }

        public void DetachTrace(Guid traceId)
        {
            var acceptable = ShouldAcceptEventForTrace(traceId);
            if (acceptable == EventAcceptability.No) return;

            var traceLeaf = (TraceLeaf?) _treeOfRtuModel.GetById(traceId);
            if (traceLeaf == null) return;
            var owner = _treeOfRtuModel.GetById(traceLeaf.Parent.Id)!;
            var rtuLeaf = owner is RtuLeaf ? (RtuLeaf) owner : (RtuLeaf) (owner.Parent);
            int port = traceLeaf.PortNumber;
            if (port <= 0)
                return;

            var detachedTraceLeaf = _globalScope.Resolve<TraceLeaf>(new NamedParameter(@"parent", rtuLeaf));
            detachedTraceLeaf.Id = traceLeaf.Id;
            detachedTraceLeaf.PortNumber = 0;
            detachedTraceLeaf.Title = traceLeaf.Title;
            detachedTraceLeaf.TraceState = FiberState.NotJoined;
            detachedTraceLeaf.TraceToTceLinkState = traceLeaf.TraceToTceLinkState;
            detachedTraceLeaf.IsInZone = acceptable == EventAcceptability.Full;
            detachedTraceLeaf.Color = acceptable == EventAcceptability.Full ? Brushes.Blue : Brushes.LightGray;

            detachedTraceLeaf.BaseRefsSet = traceLeaf.BaseRefsSet;
            detachedTraceLeaf.BaseRefsSet.IsInMonitoringCycle = false;

            ((IPortOwner) owner).ChildrenImpresario.Children.RemoveAt(port - 1);
            ((IPortOwner) owner).ChildrenImpresario.Children.Insert(port - 1,
                _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", owner), new NamedParameter(@"portNumber", port)));

            rtuLeaf.ChildrenImpresario.Children.Add(detachedTraceLeaf);
        }

        public void AddMeasurement(MeasurementAdded e)
        {
            if (!_currentUser.IsDefaultZoneUser &&
                !_readModel.Rtus.First(r => r.Id == e.RtuId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var traceLeaf = (TraceLeaf?)_treeOfRtuModel.GetById(e.TraceId);
            if (traceLeaf == null || traceLeaf.TraceState == FiberState.NotJoined)
                return;

            if (traceLeaf.TraceState == e.TraceState) return;
            traceLeaf.TraceState = e.TraceState != FiberState.Ok && e.BaseRefType == BaseRefType.Fast
                ? FiberState.Suspicion : e.TraceState;
        }

        // traces link pictogram
        public void AddOrUpdateTceWithRelation(TceWithRelationsAddedOrUpdated e)
        {
            foreach (var traceId in e.ExcludedTraceIds)
            {
                var acceptable = ShouldAcceptEventForTrace(traceId);
                if (acceptable == EventAcceptability.No) continue;

                var traceLeaf = (TraceLeaf?)_treeOfRtuModel.GetById(traceId);
                if (traceLeaf == null) return;

                traceLeaf.TraceToTceLinkState = TraceToTceLinkState.NoLink;
            }

            foreach (var portRelation in e.AllRelationsOfTce)
            {
                var acceptable = ShouldAcceptEventForTrace(portRelation.TraceId);
                if (acceptable == EventAcceptability.No) continue;

                var traceLeaf = (TraceLeaf?)_treeOfRtuModel.GetById(portRelation.TraceId);
                if (traceLeaf == null) return;

                traceLeaf.TraceToTceLinkState = e.ProcessSnmpTraps ? TraceToTceLinkState.LinkTceOn : TraceToTceLinkState.LinkTceOff;
            }
        }

        public void RemoveTce(TceRemoved e)
        {
            foreach (var traceId in e.ExcludedTraceIds)
            {
                var acceptable = ShouldAcceptEventForTrace(traceId);
                if (acceptable == EventAcceptability.No) continue;

                var traceLeaf = (TraceLeaf?)_treeOfRtuModel.GetById(traceId);
                if (traceLeaf == null) return;

                traceLeaf.TraceToTceLinkState = TraceToTceLinkState.NoLink;
            }
        }
    }
}