using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class BopStateViewModel : Screen
    {
        private readonly DataCenterConfig _currentDatacenterParameters;
        private readonly SoundManager _soundManager;
        private readonly Model _readModel;
        public Guid BopId { get; set; }
        public string BopIp { get; set; } = null!;
        public string PortRtu { get; set; } = null!;
        public string RtuTitle { get; set; } = null!;
        public string ServerTitle { get; set; } = null!;
        public string StateOn { get; set; } = null!;
        public string BopState { get; set; } = null!;
        public Brush BopStateBrush { get; set; } = null!;
        public bool IsOk { get; set; }

        private bool _isSoundForThisVmInstanceOn;
        private bool _isSoundButtonEnabled;
        public bool IsSoundButtonEnabled
        {
            get => _isSoundButtonEnabled;
            set
            {
                if (value == _isSoundButtonEnabled) return;
                _isSoundButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsOpen { get; private set; }

        public BopStateViewModel(DataCenterConfig currentDatacenterParameters, SoundManager soundManager, Model readModel)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            _soundManager = soundManager;
            _readModel = readModel;
        }

        public void Initialize(BopNetworkEventAdded bopNetworkEventAdded)
        {
            var otau = _readModel.Otaus.FirstOrDefault(o =>
                o.NetAddress.Ip4Address == bopNetworkEventAdded.OtauIp &&
                o.NetAddress.Port == bopNetworkEventAdded.TcpPort);
            if (otau == null) return;

            BopId = otau.Id;
            PortRtu = otau.MasterPort != 0 ? otau.MasterPort.ToString() : "";
            BopIp = !string.IsNullOrEmpty(otau.VeexRtuMainOtauId) && otau.VeexRtuMainOtauId.StartsWith(@"S1_") 
                ? Resources.SID_Main : bopNetworkEventAdded.OtauIp;

            var rtu = _readModel.Rtus.First(r => r.Id == bopNetworkEventAdded.RtuId);
            RtuTitle = rtu.Title;
            ServerTitle = _currentDatacenterParameters.General.ServerTitle;
            StateOn = string.Format(Resources.SID_State_at_,
                bopNetworkEventAdded.EventTimestamp.ToString(CultureInfo.CurrentCulture), bopNetworkEventAdded.Ordinal);
            IsOk = bopNetworkEventAdded.IsOk;
            BopState = bopNetworkEventAdded.IsOk ? Resources.SID_OK_BOP : Resources.SID_Bop_breakdown;
            BopStateBrush = bopNetworkEventAdded.IsOk ? Brushes.White : Brushes.Red;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_BOP_state;
            IsOpen = true;

            if (IsOk)
                _soundManager.PlayOk();
            else
            {
                _soundManager.StartAlert();
                IsSoundButtonEnabled = true;
                _isSoundForThisVmInstanceOn = true;
            }
        }

        public void TurnSoundOff()
        {
            if (_isSoundForThisVmInstanceOn)
            {
                _isSoundForThisVmInstanceOn = false;
                _soundManager.StopAlert();
                IsSoundButtonEnabled = false;
            }
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            IsOpen = false;
            await Task.Delay(0, cancellationToken);
            return true;
        }
    }
}
