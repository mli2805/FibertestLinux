using System;
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
    public class OtauToAttachViewModel : Screen
    {
        private Rtu _rtu = null!;
        private int _portNumberForAttachment;
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly IWindowManager _windowManager;

        public string RtuTitle { get; set; } = null!;
        public int RtuPortNumber { get; set; }

        private NetAddressInputViewModel _netAddressInputViewModel = null!;
        public NetAddressInputViewModel NetAddressInputViewModel
        {
            get => _netAddressInputViewModel;
            set
            {
                if (Equals(value, _netAddressInputViewModel)) return;
                _netAddressInputViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        private string _otauSerial = null!;
        public string OtauSerial
        {
            get => _otauSerial;
            set
            {
                if (value == _otauSerial) return;
                _otauSerial = value;
                NotifyOfPropertyChange();
            }
        }

        private int _otauPortCount;
        public int OtauPortCount
        {
            get => _otauPortCount;
            set
            {
                if (value == _otauPortCount) return;
                _otauPortCount = value;
                NotifyOfPropertyChange();
            }
        }

        private string _attachmentProgress = null!;
        public string AttachmentProgress
        {
            get => _attachmentProgress;
            set
            {
                if (value == _attachmentProgress) return;
                _attachmentProgress = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonsEnabled = true;
        public bool IsButtonsEnabled
        {
            get { return _isButtonsEnabled; }
            set
            {
                if (value == _isButtonsEnabled) return;
                _isButtonsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public OtauToAttachViewModel(ILifetimeScope globalScope, Model readModel, 
            IWcfServiceCommonC2D c2RWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2RWcfManager = c2RWcfManager;
            _windowManager = windowManager;
        }

        public void Initialize(Guid rtuId, int portNumberForAttachment)
        {
            _rtu = _readModel.Rtus.First(r => r.Id == rtuId);
            _portNumberForAttachment = portNumberForAttachment;
            InitializeView();
        }

        private void InitializeView()
        {
            RtuTitle = _rtu.Title;
            RtuPortNumber = _portNumberForAttachment;
            OtauSerial = "";
            OtauPortCount = 0;
            AttachmentProgress = "";

            NetAddressInputViewModel = new NetAddressInputViewModel(
                new NetAddress()
                {
                    Ip4Address = _rtu.RtuMaker == RtuMaker.IIT ?  @"192.168.96.57" : @"192.168.96.237", 
                    Port = _rtu.RtuMaker == RtuMaker.IIT ? 11834 : 4001, 
                    IsAddressSetAsIp = true
                }, true);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Attach_optical_switch;
        }

        public async Task AttachOtau()
        {
            IsButtonsEnabled = false;
            if (! await CheckAddressUniqueness())
            {
                IsButtonsEnabled = true;
                return;
            }

            OtauAttachedDto result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                AttachmentProgress = Resources.SID_Please__wait_;
                result = await AttachOtauIntoRtu();
            }
            if (result.IsAttached)
            {
                AttachmentProgress = Resources.SID_Successful_;
                OtauSerial = _rtu.RtuMaker == RtuMaker.IIT
                    ? result.Serial!.Substring(0, result.Serial.Length - 1)
                    : result.Serial!;
                OtauPortCount = result.PortCount;
            }
            else
            {
                AttachmentProgress = Resources.SID_Failed_;
                var strs = new List<string>()
                {
                    $@"{result.ReturnCode.GetLocalizedString()}",
                    "",
                    result.ErrorMessage!,
                };
                var vm = new MyMessageBoxViewModel(MessageType.Error, strs, 0);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }

            IsButtonsEnabled = true;
        }

        private async Task<OtauAttachedDto> AttachOtauIntoRtu()
        {
            var netAddress = new NetAddress(NetAddressInputViewModel.GetNetAddress().Ip4Address,
                NetAddressInputViewModel.GetNetAddress().Port);
            var dto = new AttachOtauDto(_rtu.Id, _rtu.RtuMaker)
            {
                OtauId = Guid.NewGuid(), // Veex Rtu will replace this by its own id
                NetAddress = netAddress,
                OpticalPort = _portNumberForAttachment
            };
            var result = await _c2RWcfManager.AttachOtauAsync(dto);
            return result;
        }

        private async Task<bool> CheckAddressUniqueness()
        {
            if (!_readModel.Otaus.Any(o =>
                o.NetAddress.Ip4Address == NetAddressInputViewModel.GetNetAddress().Ip4Address &&
                o.NetAddress.Port == NetAddressInputViewModel.GetNetAddress().Port))
                return true;

            var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_There_is_optical_switch_with_the_same_tcp_address_);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            return false;
        }

        public async void Close()
        {
            await TryCloseAsync();
        }
    }
}
