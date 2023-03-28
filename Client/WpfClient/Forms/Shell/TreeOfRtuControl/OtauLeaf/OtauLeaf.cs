using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class OtauLeaf : Leaf, IPortOwner
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private RtuPartState _otauState;
        public int OwnPortCount { get; set; }
        public int MasterPort { get; set; }
        public NetAddress? OtauNetAddress { get; set; }
        public string? Serial { get; set; }
        public string OtauId { get; set; } = "";

        public RtuPartState OtauState
        {
            get => _otauState;
            set
            {
                if (value == _otauState) return;
                _otauState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(OtauStatePictogram));
            }
        }

        public string OtauStatePictogram => OtauState.GetPathToPictogram();

        public bool HasAttachedTraces =>
            ChildrenImpresario.Children.Any(l => l is TraceLeaf && ((TraceLeaf)l).PortNumber > 0);

        public override string Name => string.Format(Resources.SID_Port_trace, MasterPort, Title);

        public ChildrenImpresario ChildrenImpresario { get; }
        public int TraceCount => ChildrenImpresario.Children.Count(c => c is TraceLeaf);

        public OtauLeaf(ILifetimeScope globalScope, Model readModel, FreePorts freePorts, CurrentUser currentUser,
            IWcfServiceCommonC2D c2RWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _c2RWcfManager = c2RWcfManager;
            _windowManager = windowManager;
            _readModel = readModel;
            _currentUser = currentUser;
            ChildrenImpresario = new ChildrenImpresario(freePorts);
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
            IsSelected = true;

            return new List<MenuItemVm>
            {
                new MenuItemVm()
                {
                    Header = Resources.SID_Remove,
                    Command = new ContextMenuAsyncAction(RemoveOtau, CanOtauRemoveAction),
                    CommandParameter = this
                }
            };
        }

        public async Task RemoveOtau(object param)
        {
            var rtu = _readModel.Rtus.First(r => r.Id == Parent.Id);

            var dto = new DetachOtauDto(rtu.Id, rtu.RtuMaker)
            {
                OtauId = Id,
                OpticalPort = MasterPort,
                NetAddress = OtauNetAddress!.Clone(),
            };
            OtauDetachedDto result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                result = await _c2RWcfManager.DetachOtauAsync(dto);
            }

            if (!result.IsDetached)
            {
                var lines = new List<string>()
                {
                    result.ReturnCode.GetLocalizedString(),
                    "",
                    result.ErrorMessage ?? "",
                };
                var vm = new MyMessageBoxViewModel(MessageType.Error, lines, 0);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        private bool CanOtauRemoveAction(object param)
        {
            if (!(param is OtauLeaf otauLeaf))
                return false;

            var rtuLeaf = (RtuLeaf)otauLeaf.Parent;

            return _currentUser.Role <= Role.Root
                   && rtuLeaf.IsAvailable
                   && rtuLeaf.MonitoringState == MonitoringState.Off;
        }
    }
}