using System.Linq;
using System.Windows.Media;
using Autofac;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class ZoneEventsOnTreeExecutor
    {
        private readonly ILifetimeScope _globalScope;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;

        public ZoneEventsOnTreeExecutor(ILifetimeScope globalScope, TreeOfRtuModel treeOfRtuModel, CurrentUser currentUser, Model readModel)
        {
            _globalScope = globalScope;
            _treeOfRtuModel = treeOfRtuModel;
            _currentUser = currentUser;
            _readModel = readModel;
        }

        public void RenderOfModelAfterSnapshot()
        {
            foreach (var rtu in _readModel.Rtus)
            {
                if (rtu.ZoneIds.Contains(_currentUser.ZoneId))
                    AddRtuLeafAndAllIts(rtu);
            }
        }

        public void ChangeResponsibility(ResponsibilitiesChanged e)
        {
            if (_currentUser.IsDefaultZoneUser) return;

            foreach (var pair in e.ResponsibilitiesDictionary)
            {
                var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == pair.Key);
                if (rtu != null) ConsiderRtuZonesChanges(rtu);
                else
                {
                    var trace = _readModel.Traces.First(t => t.TraceId == pair.Key);
                    ConsiderTraceZonesChanges(trace);
                }
            }
        }

        private void ConsiderRtuZonesChanges(Rtu rtu)
        {
            var rtuLeaf = _treeOfRtuModel.GetById(rtu.Id);
            // for current user this RTU belonging doesn't changed 
            if (rtuLeaf == null ^ rtu.ZoneIds.Contains(_currentUser.ZoneId)) return;

            if (rtuLeaf == null)
                AddRtuLeafAndAllIts(rtu);
            else
                _treeOfRtuModel.Tree.Remove(rtuLeaf);
        }

        private void AddRtuLeafAndAllIts(Rtu rtu)
        {
            var rtuLeaf = _globalScope.Resolve<RtuLeaf>();
            rtuLeaf.Id = rtu.Id;
            rtuLeaf.Title = rtu.Title;
            rtuLeaf.Color = string.IsNullOrEmpty(rtu.Serial) ? Brushes.DarkGray : Brushes.Black;
            rtuLeaf.RtuMaker = rtu.RtuMaker;

            rtuLeaf.IsMainOtauOk = rtu.RtuMaker == RtuMaker.IIT || (rtu.MainVeexOtau?.connected ?? false);
            rtuLeaf.OwnPortCount = rtu.OwnPortCount;
            rtuLeaf.FullPortCount = rtu.FullPortCount;
            rtuLeaf.Serial = rtu.Serial;
            rtuLeaf.OtauNetAddress = rtu.OtdrNetAddress;

            rtuLeaf.MainChannelState = rtu.MainChannelState;
            rtuLeaf.ReserveChannelState = rtu.ReserveChannelState;
            rtuLeaf.MonitoringState = rtu.MonitoringState;

            rtuLeaf.TreeOfAcceptableMeasParams = rtu.AcceptableMeasParams;

            for (int i = 1; i <= rtuLeaf.OwnPortCount; i++)
            {
                var port = _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", rtuLeaf), new NamedParameter(@"portNumber", i));
                rtuLeaf.ChildrenImpresario.Children.Insert(i - 1, port);
                port.Parent = rtuLeaf;
            }

            foreach (var otau in _readModel.Otaus.Where(o => o.RtuId == rtu.Id))
            {
                if (rtu.RtuMaker == RtuMaker.VeEX && (otau.VeexRtuMainOtauId != null || otau.IsMainOtau))
                    continue;

                var otauLeaf = _globalScope.Resolve<OtauLeaf>();
                otauLeaf.Id = otau.Id;
                otauLeaf.Parent = rtuLeaf;
                otauLeaf.Serial = otau.Serial;
                otauLeaf.OtauId = otau.Id.ToString();
                otauLeaf.Title = string.Format(Resources.SID_Optical_switch_with_Address,  otau.NetAddress?.ToStringA());
                otauLeaf.Color = Brushes.Black;
                otauLeaf.MasterPort = otau.MasterPort;
                otauLeaf.OwnPortCount = otau.PortCount;
                otauLeaf.OtauNetAddress = otau.NetAddress;
                otauLeaf.OtauState = otau.IsOk ? RtuPartState.Ok : RtuPartState.Broken;
                otauLeaf.IsExpanded = true;

                for (int i = 0; i < otauLeaf.OwnPortCount; i++)
                    otauLeaf.ChildrenImpresario.Children.Add(
                        _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", otauLeaf), new NamedParameter(@"portNumber", i + 1)));
                rtuLeaf.ChildrenImpresario.Children.Remove(rtuLeaf.ChildrenImpresario.Children[otau.MasterPort - 1]);
                rtuLeaf.ChildrenImpresario.Children.Insert(otau.MasterPort - 1, otauLeaf);
                rtuLeaf.SetOtauState(otau.Id, otau.IsOk);
            }

            foreach (var trace in _readModel.Traces.Where(t => t.RtuId == rtu.Id))
            {
                var traceLeaf = _globalScope.Resolve<TraceLeaf>(new NamedParameter(@"parent", rtuLeaf));
                var portOwner = trace.OtauPort == null ? rtuLeaf : rtuLeaf.GetPortOwner(trace.OtauPort);
                if (portOwner == null) continue;

                traceLeaf.Id = trace.TraceId;
                traceLeaf.Parent = (Leaf)portOwner;
                traceLeaf.Title = trace.Title;
                traceLeaf.PortNumber = trace.OtauPort?.OpticalPort ?? -1;
                traceLeaf.TraceState = trace.State;
                traceLeaf.TraceToTceLinkState = trace.TraceToTceLinkState;
                traceLeaf.IsInZone = trace.ZoneIds.Contains(_currentUser.ZoneId);
                traceLeaf.BaseRefsSet = new BaseRefsSet()
                {
                    PreciseId = trace.PreciseId, PreciseDuration = trace.PreciseDuration,
                    FastId = trace.FastId, FastDuration = trace.FastDuration,
                    AdditionalId = trace.AdditionalId, AdditionalDuration = trace.AdditionalDuration,
                    IsInMonitoringCycle = trace.IsIncludedInMonitoringCycle,
                    RtuMonitoringState = rtu.MonitoringState,
                };

                if (traceLeaf.PortNumber > 0)
                {
                    portOwner.ChildrenImpresario.Children[traceLeaf.PortNumber - 1] = traceLeaf;
                }
                else
                    rtuLeaf.ChildrenImpresario.Children.Add(traceLeaf);
            }

            _treeOfRtuModel.Tree.Add(rtuLeaf);
        }

        private void ConsiderTraceZonesChanges(Trace trace)
        {
            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(trace.RtuId);
            if (rtuLeaf == null) return; // RTU not in zone
            var traceLeaf = (TraceLeaf)_treeOfRtuModel.GetById(trace.TraceId);
            if (traceLeaf == null)
            {
                traceLeaf = _globalScope.Resolve<TraceLeaf>(new NamedParameter(@"parent", rtuLeaf));
                var portOwner = trace.OtauPort == null ? rtuLeaf : rtuLeaf.GetPortOwner(trace.OtauPort);
                if (portOwner == null) return;

                traceLeaf.Id = trace.TraceId;
                traceLeaf.Parent = (Leaf)portOwner;
                traceLeaf.Title = trace.Title;
                traceLeaf.PortNumber = trace.OtauPort?.OpticalPort ?? -1;
                traceLeaf.TraceState = trace.State;
                traceLeaf.IsInZone = trace.ZoneIds.Contains(_currentUser.ZoneId);
                traceLeaf.BaseRefsSet = new BaseRefsSet()
                {
                    PreciseId = trace.PreciseId, PreciseDuration = trace.PreciseDuration,
                    FastId = trace.FastId, FastDuration = trace.FastDuration,
                    AdditionalId = trace.AdditionalId, AdditionalDuration = trace.AdditionalDuration,
                    IsInMonitoringCycle = trace.IsIncludedInMonitoringCycle,
                };

                if (traceLeaf.PortNumber > 0)
                {
                    portOwner.ChildrenImpresario.Children[traceLeaf.PortNumber - 1] = traceLeaf;

                }
                else
                    rtuLeaf.ChildrenImpresario.Children.Add(traceLeaf);
            }

            traceLeaf.IsInZone = trace.ZoneIds.Contains(_currentUser.ZoneId);
        }
    }
}