using System.Windows.Controls;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class RtuVmContextMenuProvider
    {
        private readonly RtuVmActions _rtuVmActions;
        private readonly CommonVmActions _commonVmActions;
        private readonly RtuVmPermissions _rtuVmPermissions;
        private readonly CurrentGis _currentGis;

        public RtuVmContextMenuProvider(RtuVmActions rtuVmActions, CommonVmActions commonVmActions, RtuVmPermissions rtuVmPermissions,
            CurrentGis currentGis)
        {
            _rtuVmActions = rtuVmActions;
            _commonVmActions = commonVmActions;
            _rtuVmPermissions = rtuVmPermissions;
            _currentGis = currentGis;
        }

        public ContextMenu GetRtuContextMenu(MarkerControl marker)
        {
            var contextMenu = new ContextMenu();

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAsyncAction(_rtuVmActions.AskUpdateRtu, _rtuVmPermissions.CanUpdateRtu),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Remove,
                Command = new ContextMenuAsyncAction(_rtuVmActions.AskRemoveRtu, _rtuVmPermissions.CanRemoveRtu),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Section,
                Command = new ContextMenuAsyncAction(_commonVmActions.StartAddFiber, _rtuVmPermissions.CanStartAddFiber),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Section_with_nodes,
                Command = new ContextMenuAsyncAction(_commonVmActions.StartAddFiberWithNodes, _rtuVmPermissions.CanStartAddFiberWithNodes),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new Separator());
            // contextMenu.Items.Add(new MenuItem()
            // {
            //     Header = Resources.SID_Define_trace,
            //     Command = new ContextMenuAsyncAction(_rtuVmActions.StartDefineTrace, _rtuVmPermissions.CanStartDefineTrace),
            //     CommandParameter = marker
            // });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Define_trace_step_by_step,
                Command = new ContextMenuAsyncAction(_rtuVmActions.StartDefineTraceStepByStep, _rtuVmPermissions.CanStartDefineTraceStepByStep),
                CommandParameter = marker
            });
            if (!_currentGis.IsHighDensityGraph)
                contextMenu.Items.Add(new MenuItem()
                {
                    Header = Resources.SID_Reveal_traces,
                    Command = new ContextMenuAsyncAction(_rtuVmActions.RevealTraces, _rtuVmPermissions.CanRevealTraces),
                    CommandParameter = marker,
                });
            return contextMenu;
        }
    }
}