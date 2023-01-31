using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

// ReSharper disable InconsistentNaming

namespace Fibertest.WpfClient
{
    public class RtuChannelViewModel : Screen
    {
        private readonly SoundManager _soundManager;
        private readonly Model _readModel;
        public string RtuTitle { get; set; }
        public string RtuAvailability { get; set; }
        public Brush AvailabilityBrush { get; set; }

        public string StateOn { get; set; }

        public string OnMainChannel { get; set; }
        public Brush OnMainChannelBrush { get; set; }
        public Visibility MainChannelVisibility { get; set; }

        public string OnReserveChannel { get; set; }
        public Brush OnReserveChannelBrush { get; set; }
        public Visibility ReserveChannelVisibility { get; set; }


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

        public bool IsOk { get; set; }
        public bool IsOpen { get; private set; }

        public RtuChannelViewModel(SoundManager soundManager, Model readModel)
        {
            _soundManager = soundManager;
            _readModel = readModel;
        }

        public void Initialize(NetworkEventAdded networkEventAdded)
        {
            var rtu = _readModel.Rtus.First(r => r.Id == networkEventAdded.RtuId);
            RtuTitle = rtu.Title;
            RtuAvailability = rtu.IsAvailable ? Resources.SID_RTU_is_available : Resources.SID_RTU_is_not_available;
            AvailabilityBrush = rtu.IsAvailable ? Brushes.Black : Brushes.Red;

            if (networkEventAdded.OnMainChannel == ChannelEvent.Nothing)
                MainChannelVisibility = Visibility.Collapsed;
            else
            {
                MainChannelVisibility = Visibility.Visible;
                OnMainChannel = networkEventAdded.OnMainChannel == ChannelEvent.Broken
                    ? Resources.SID_Main_channel_is_broken
                    : Resources.SID_Main_channel_is_Ok;
                OnMainChannelBrush = networkEventAdded.OnMainChannel == ChannelEvent.Repaired ? Brushes.White : Brushes.Red;
            }

            if (networkEventAdded.OnReserveChannel == ChannelEvent.Nothing)
                ReserveChannelVisibility = Visibility.Collapsed;
            else
            {
                ReserveChannelVisibility = Visibility.Visible;
                OnReserveChannel = networkEventAdded.OnReserveChannel == ChannelEvent.Broken
                    ? Resources.SID_Reserve_channel_is_broken
                    : Resources.SID_Reserve_channel_is_Ok;
                OnReserveChannelBrush = networkEventAdded.OnReserveChannel == ChannelEvent.Repaired ? Brushes.White : Brushes.Red;
            }

            IsOk = networkEventAdded.OnMainChannel != ChannelEvent.Broken &&
                   networkEventAdded.OnReserveChannel != ChannelEvent.Broken;
            StateOn = string.Format(Resources.SID_State_at_,
                networkEventAdded.EventTimestamp.ToString(CultureInfo.CurrentCulture), networkEventAdded.Ordinal);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_RTU_connection_state;
            IsOpen = true;

            if (IsOk)
                _soundManager.PlayOk();
            else
            {
                _isSoundForThisVmInstanceOn = true;
                _soundManager.StartAlert();
                IsSoundButtonEnabled = true;
            }
        }

        public void TurnSoundOff()
        {
            if (_isSoundForThisVmInstanceOn)
            {
                _soundManager.StopAlert();
                _isSoundForThisVmInstanceOn = false;
                IsSoundButtonEnabled = false;
            }
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            IsOpen = false;
            return true;
        }
    }
}
