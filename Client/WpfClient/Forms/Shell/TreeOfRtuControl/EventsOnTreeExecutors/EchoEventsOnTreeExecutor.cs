using System;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class EchoEventsOnTreeExecutor
    {
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;

        public EchoEventsOnTreeExecutor(TreeOfRtuModel treeOfRtuModel, CurrentUser currentUser, Model readModel)
        {
            _treeOfRtuModel = treeOfRtuModel;
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

        public void AssignBaseRef(BaseRefAssigned e)
        {
            if (ShouldAcceptEventForTrace(e.TraceId) == EventAcceptability.No) return;

            var traceLeaf = (TraceLeaf?)_treeOfRtuModel.GetById(e.TraceId);
            if (traceLeaf == null) return;

            var preciseBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
            if (preciseBaseRef != null)
            {
                traceLeaf.BaseRefsSet.PreciseId = preciseBaseRef.Id;
                traceLeaf.BaseRefsSet.PreciseDuration = preciseBaseRef.Duration;
            }
            var fastBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Fast);
            if (fastBaseRef != null)
            {
                traceLeaf.BaseRefsSet.FastId = fastBaseRef.Id;
                traceLeaf.BaseRefsSet.FastDuration = fastBaseRef.Duration;
            }
            var additionalBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Additional);
            if (additionalBaseRef != null)
            {
                traceLeaf.BaseRefsSet.AdditionalId = additionalBaseRef.Id;
                traceLeaf.BaseRefsSet.AdditionalDuration = additionalBaseRef.Duration;
            }

            if (!traceLeaf.BaseRefsSet.HasEnoughBaseRefsToPerformMonitoring)
                traceLeaf.BaseRefsSet.IsInMonitoringCycle = false;
        }

     
        public void ChangeMonitoringSettings(MonitoringSettingsChanged e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.RtuId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf?)_treeOfRtuModel.GetById(e.RtuId);
            if (rtuLeaf == null) return;
            rtuLeaf.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
            ApplyMonitoringSettingsRecursively(rtuLeaf, e);
        }

        private void ApplyMonitoringSettingsRecursively(IPortOwner portOwner, MonitoringSettingsChanged e)
        {
            foreach (var leaf in portOwner.ChildrenImpresario.Children)
            {
                if (leaf is TraceLeaf traceLeaf)
                {
                    traceLeaf.BaseRefsSet.IsInMonitoringCycle = e.TracesInMonitoringCycle.Contains(traceLeaf.Id);
                    traceLeaf.BaseRefsSet.RtuMonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
                }

                if (leaf is OtauLeaf otauLeaf)
                    ApplyMonitoringSettingsRecursively(otauLeaf, e);
            }

        }

        public void StartMonitoring(MonitoringStarted e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.RtuId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf?)_treeOfRtuModel.GetById(e.RtuId);
            if (rtuLeaf == null) return;
            rtuLeaf.MonitoringState = MonitoringState.On;
            ApplyRecursively(rtuLeaf, MonitoringState.On);
        }

        public void StopMonitoring(MonitoringStopped e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.RtuId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf?)_treeOfRtuModel.GetById(e.RtuId);
            if (rtuLeaf == null) return;
            rtuLeaf.MonitoringState = MonitoringState.Off;
            ApplyRecursively(rtuLeaf, MonitoringState.Off);
        }

        private void ApplyRecursively(IPortOwner portOwner, MonitoringState rtuMonitoringState)
        {
            foreach (var leaf in portOwner.ChildrenImpresario.Children)
            {
                if (leaf is TraceLeaf traceLeaf)
                    traceLeaf.BaseRefsSet.RtuMonitoringState = rtuMonitoringState;

                if (leaf is OtauLeaf otauLeaf)
                    ApplyRecursively(otauLeaf, rtuMonitoringState);
            }
        }
    }
}