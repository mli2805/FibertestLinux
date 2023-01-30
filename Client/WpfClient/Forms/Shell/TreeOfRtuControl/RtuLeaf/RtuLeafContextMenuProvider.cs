using System.Collections.Generic;
using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class RtuLeafContextMenuProvider
    {
        private readonly CurrentUser _currentUser;
        private readonly RtuLeafActions _rtuLeafActions;
        private readonly RtuLeafActionsPermissions _rtuLeafActionsPermissions;

        public RtuLeafContextMenuProvider(CurrentUser currentUser,
            RtuLeafActions rtuLeafActions, RtuLeafActionsPermissions rtuLeafActionsPermissions)
        {
            _currentUser = currentUser;
            _rtuLeafActions = rtuLeafActions;
            _rtuLeafActionsPermissions = rtuLeafActionsPermissions;
        }

        public List<MenuItemVm> GetMenu(RtuLeaf rtuLeaf)
        {
            var menu = new List<MenuItemVm>();
            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.ShowRtuInfoView, _rtuLeafActionsPermissions.CanShowRtuInfoView),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Show_RTU,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.HighlightRtu, _rtuLeafActionsPermissions.CanHighlightRtu),
                CommandParameter = rtuLeaf
            });

            if (_currentUser.Role == Role.Developer)
                menu.Add(new MenuItemVm()
                {
                    Header = @"Export to file",
                    Command = new ContextMenuAsyncAction(_rtuLeafActions.ExportRtuToFile, _rtuLeafActionsPermissions.CanExportRtuToFile),
                    CommandParameter = rtuLeaf
                });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Network_settings,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.InitializeRtu, _rtuLeafActionsPermissions.CanInitializeRtu),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_State,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.ShowRtuState, _rtuLeafActionsPermissions.CanShowRtuState),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Landmarks,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.ShowRtuLandmarks, _rtuLeafActionsPermissions.CanShowRtuLandmarks),
                CommandParameter = rtuLeaf
            });

            menu.Add(null);

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Assign_base_refs_automatically,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.AssignBaseRefsAutomatically, _rtuLeafActionsPermissions.CanAssignBaseRefsAutomatically),
                CommandParameter = rtuLeaf
            });

            menu.Add(null);

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Monitoring_settings,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.ShowMonitoringSettings, _rtuLeafActionsPermissions.CanShowMonitoringSettings),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Manual_mode,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.StopMonitoring, _rtuLeafActionsPermissions.CanStopMonitoring),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Automatic_mode,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.StartMonitoring, _rtuLeafActionsPermissions.CanStartMonitoring),
                CommandParameter = rtuLeaf
            });

            menu.Add(null);

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Detach_all_traces,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.DetachAllTraces, _rtuLeafActionsPermissions.CanDetachAllTraces),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Remove,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.RemoveRtu, _rtuLeafActionsPermissions.CanRemoveRtu),
                CommandParameter = rtuLeaf
            });

            menu.Add(null);

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Define_trace_step_by_step,
                Command = new ContextMenuAsyncAction(_rtuLeafActions.DefineTraceStepByStep, _rtuLeafActionsPermissions.CanDefineTraceStepByStep),
                CommandParameter = rtuLeaf
            });
            return menu;
        }
    }
}