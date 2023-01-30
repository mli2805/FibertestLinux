using System;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class InitializeRtuEventOnTreeExecutor
    {
        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly TreeOfRtuModel _treeOfRtuModel;

        public InitializeRtuEventOnTreeExecutor(ILifetimeScope globalScope, CurrentUser currentUser,
            Model readModel, TreeOfRtuModel treeOfRtuModel)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _readModel = readModel;
            _treeOfRtuModel = treeOfRtuModel;
        }

        public void InitializeRtu(RtuInitialized e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.Id).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.Id);

            if (rtuLeaf.Serial == null)
                InitializeFirstTime(rtuLeaf, e);
            else
                ReInitialize(rtuLeaf, e);

            rtuLeaf.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
            foreach (var child in rtuLeaf.ChildrenImpresario.Children)
            {
                if (child is TraceLeaf traceLeaf)
                    traceLeaf.BaseRefsSet.RtuMonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
                else if (child is OtauLeaf otauLeaf)
                    foreach (var leaf in otauLeaf.ChildrenImpresario.Children)
                    {
                        if (leaf is TraceLeaf traceLeaf1)
                            traceLeaf1.BaseRefsSet.RtuMonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
                    }
            }
        }

        private void ReInitialize(RtuLeaf rtuLeaf, RtuInitialized e)
        {

            if (rtuLeaf.OwnPortCount < e.OwnPortCount)
            {
                for (int i = rtuLeaf.OwnPortCount + 1; i <= e.OwnPortCount; i++)
                {
                    var port = _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", rtuLeaf), new NamedParameter(@"portNumber", i));
                    rtuLeaf.ChildrenImpresario.Children.Insert(i - 1, port);
                    port.Parent = rtuLeaf;
                }
            }

            if (rtuLeaf.OwnPortCount > e.OwnPortCount)
            {
                for (int i = rtuLeaf.OwnPortCount - 1; i >= e.OwnPortCount; i--)
                {
                    rtuLeaf.ChildrenImpresario.Children.RemoveAt(i);
                }
            }

            if (e.Children != null)
                foreach (var childPair in e.Children)
                {
                    var otau = rtuLeaf.ChildrenImpresario.Children.Select(child => child as OtauLeaf)
                        .FirstOrDefault(o => o?.OtauNetAddress?.Equals(childPair.Value.NetAddress) == true);

                    /*
                     RTU cannot return child OTAU which does not exist yet! It's a business rule
                     Client sends existing OTAU list -> 
                     RTU MUST detach any OTAU which are not in client's list
                     and attach all OTAU from this list
                    */
                    if (otau != null)
                        rtuLeaf.SetOtauState(otau.Id, childPair.Value.IsOk);
                }

            SetRtuProperties(rtuLeaf, e);
        }

      
        private void InitializeFirstTime(RtuLeaf rtuLeaf, RtuInitialized e)
        {
            SetRtuProperties(rtuLeaf, e);

            for (int i = 1; i <= e.OwnPortCount; i++)
            {
                var port = _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", rtuLeaf), new NamedParameter(@"portNumber", i));
                rtuLeaf.ChildrenImpresario.Children.Insert(i - 1, port);
                port.Parent = rtuLeaf;
            }

            /*
             While first initialization RTU cannot return children! It's a business rule
             RTU MUST detach all OTAUs if has any
            */
        }

        private static void SetRtuProperties(RtuLeaf rtuLeaf, RtuInitialized e)
        {
            rtuLeaf.RtuMaker = e.Maker;
            rtuLeaf.OwnPortCount = e.OwnPortCount;
            rtuLeaf.FullPortCount = e.FullPortCount;
            rtuLeaf.Serial = e.Serial;
            rtuLeaf.MainChannelState = e.MainChannelState;
            rtuLeaf.ReserveChannelState = e.ReserveChannelState;
            rtuLeaf.IsMainOtauOk = e.MainVeexOtau.connected;
            rtuLeaf.OtauNetAddress = e.OtauNetAddress ?? new NetAddress("", -1);
            rtuLeaf.Color = Brushes.Black;
            rtuLeaf.TreeOfAcceptableMeasParams = e.AcceptableMeasParams ?? new TreeOfAcceptableMeasParams();
        }
    }
}