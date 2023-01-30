using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class RtuRemover
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;

        public RtuRemover(ILifetimeScope globalScope, IWindowManager windowManager, IWcfServiceDesktopC2D c2DWcfManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
        }

        public async Task<string> Fire(Rtu rtu)
        {
            if (!await _globalScope.Resolve<IRtuHolder>().SetRtuOccupationState(rtu.Id, rtu.Title, RtuOccupation.RemoveRtu))
                return @"RTU is busy";

            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, string.Format(Resources.SID_Remove_RTU___0____, rtu.Title));
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            if (!vm.IsAnswerPositive) return null;
            var cmd = new RemoveRtu() { RtuId = rtu.Id, RtuNodeId = rtu.NodeId };
            return await _c2DWcfManager.SendCommandAsObj(cmd);
        }
    }
}