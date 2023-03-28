using System;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public class TraceLeafActionsPermissions
    {
        private readonly CurrentUser _currentUser;

        public TraceLeafActionsPermissions(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public bool CanUpdateTrace(object param) { return true; }

        public bool CanShowTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;

            return traceLeaf.IsInZone;
        }

        public bool CanHighlightTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;

            return traceLeaf.IsInZone;
        }

        public bool CanRevealTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;

            return traceLeaf.IsInZone;
        }

        // if trace attached to port and RTU is not available now - it is prohibited to assign base - you can't send base to RTU
        public bool CanAssignBaseRefsAction(object param)
        {
            if (_currentUser.Role > Role.Operator)
                return false;
            if (!(param is TraceLeaf traceLeaf) || !traceLeaf.IsInZone)
                return false;


            var leaf = traceLeaf.Parent as RtuLeaf;
            var rtuLeaf = leaf ?? (RtuLeaf)traceLeaf.Parent.Parent;

            if (rtuLeaf.TreeOfAcceptableMeasParams == null) // RTU is not initialized yet
                return false;

            return traceLeaf.PortNumber < 1
                    || rtuLeaf.IsAvailable &&
                   (traceLeaf.BaseRefsSet.RtuMonitoringState == MonitoringState.Off
                   || !traceLeaf.BaseRefsSet.IsInMonitoringCycle);
        }

        public bool CanShowTraceState(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;

            return traceLeaf.IsInZone;
        }

        public bool CanShowTraceStatistics(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;

            return traceLeaf.IsInZone;
        }

        public bool CanShowTraceLandmarks(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;

            return traceLeaf.IsInZone;
        }

        public bool CanDetachTrace(object param)
        {
            if (_currentUser.Role > Role.Operator)
                return false;
            if (!(param is TraceLeaf traceLeaf) || !traceLeaf.IsInZone)
                return false;

            return traceLeaf.BaseRefsSet.RtuMonitoringState == MonitoringState.Off
                                || !traceLeaf.BaseRefsSet.IsInMonitoringCycle;
        }

        public bool CanCleanTrace(object param)
        {
            return _currentUser.Role <= Role.Root;
        }

        public bool CanRemoveTrace(object param)
        {
            return _currentUser.Role <= Role.Root;
        }

        public bool CanDoPreciseMeasurementOutOfTurn(object param)
        {
            if (_currentUser.Role > Role.Operator)
                return false;

            if (!(param is TraceLeaf traceLeaf) || !traceLeaf.IsInZone)
                return false;

            if (traceLeaf.Parent is OtauLeaf otauLeaf && otauLeaf.OtauState != RtuPartState.Ok)
                return false;

            var parent = traceLeaf.Parent as RtuLeaf;
            var rtuLeaf = parent ?? (RtuLeaf)traceLeaf.Parent.Parent;
            if (!rtuLeaf.IsAvailable)
                return false;

            return traceLeaf.PortNumber > 0 && traceLeaf.BaseRefsSet.PreciseId != Guid.Empty;
        }

        public bool CanAssignBaseRefsAutomatically(object param)
        {
            if (_currentUser.Role > Role.Operator)
                return false;

            if (!(param is TraceLeaf traceLeaf) || !traceLeaf.IsInZone)
                return false;

            if (traceLeaf.Parent is OtauLeaf otauLeaf && otauLeaf.OtauState != RtuPartState.Ok)
                return false;

            var parent = traceLeaf.Parent as RtuLeaf;
            var rtuLeaf = parent ?? (RtuLeaf)traceLeaf.Parent.Parent;

            return traceLeaf.PortNumber > 0 && rtuLeaf.IsAvailable && 
                   (traceLeaf.BaseRefsSet.RtuMonitoringState == MonitoringState.Off
                                         || !traceLeaf.BaseRefsSet.IsInMonitoringCycle);
        }
    }
}