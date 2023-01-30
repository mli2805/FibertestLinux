using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class RtuLeafActionsPermissions
    {
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;

        public RtuLeafActionsPermissions(CurrentUser currentUser, Model readModel)
        {
            _currentUser = currentUser;
            _readModel = readModel;
        }

        public bool CanShowRtuInfoView(object param) { return true; }

        public bool CanHighlightRtu(object param) { return true; }
        public bool CanExportRtuToFile(object param) { return true; }

        public bool CanInitializeRtu(object param) { return param is RtuLeaf; }

        public bool CanShowRtuState(object param)
        {
            return param is RtuLeaf rtuLeaf && rtuLeaf.MainChannelState != RtuPartState.NotSetYet;
        }

        public bool CanShowRtuLandmarks(object param)
        {
            if (!(param is RtuLeaf rtuLeaf)) return false;
            return _readModel.Traces.Any(t => t.RtuId == rtuLeaf.Id);
        }

        public bool CanShowMonitoringSettings(object param)
        {
            return param is RtuLeaf rtuLeaf && rtuLeaf.IsAvailable;
        }

        public bool CanStartMonitoring(object param)
        {
            return _currentUser.Role <= Role.Operator 
                   && param is RtuLeaf rtuLeaf 
                   && rtuLeaf.IsAvailable 
                   && rtuLeaf.MonitoringState == MonitoringState.Off;
        }

        public bool CanStopMonitoring(object param)
        {
            return _currentUser.Role <= Role.Operator
                   && param is RtuLeaf rtuLeaf
                   && rtuLeaf.IsAvailable
                   && rtuLeaf.MonitoringState == MonitoringState.On;
        }

        public bool CanDetachAllTraces(object param)
        {
            return _currentUser.Role <= Role.Operator
                   && param is RtuLeaf rtuLeaf
                   && rtuLeaf.IsAvailable
                   && rtuLeaf.MonitoringState == MonitoringState.Off;
       
        }

        public bool CanRemoveRtu(object param)
        {
            return _currentUser.Role <= Role.Root 
                   && param is RtuLeaf rtuLeaf 
                   && (!rtuLeaf.HasAttachedTraces || !rtuLeaf.IsAvailable);
        }

        public bool CanDefineTraceStepByStep(object param)
        {
            return _currentUser.Role <= Role.Root;
        }

        public bool CanAssignBaseRefsAutomatically(object param)
        {
            return _currentUser.Role <= Role.Operator
                   && param is RtuLeaf rtuLeaf
                   && rtuLeaf.IsAvailable
                   && rtuLeaf.MonitoringState == MonitoringState.Off;
        }
    }
}