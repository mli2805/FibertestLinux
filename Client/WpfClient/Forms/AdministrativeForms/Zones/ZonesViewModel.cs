using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ZonesViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        private ObservableCollection<Zone> _rows;

        public ObservableCollection<Zone> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        private Zone _selectedZone;
        public Zone SelectedZone
        {
            get { return _selectedZone; }
            set
            {
                if (Equals(value, _selectedZone)) return;
                _selectedZone = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsRemoveEnabled));
            }
        }

        public bool IsRemoveEnabled => !SelectedZone.IsDefaultZone;

        public bool IsEnabled { get; set; }

        public ZonesViewModel(ILifetimeScope globalScope, Model readModel, EventArrivalNotifier eventArrivalNotifier,
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager, CurrentUser currentUser)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            Rows = new ObservableCollection<Zone>(readModel.Zones);
            SelectedZone = Rows.First();
            eventArrivalNotifier.PropertyChanged += _eventArrivalNotifier_PropertyChanged;
            IsEnabled = currentUser.Role <= Role.Root;
        }

        private void _eventArrivalNotifier_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Rows = new ObservableCollection<Zone>(_readModel.Zones);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones;
        }

        public async void AddZone()
        {
            var vm = _globalScope.Resolve<ZoneViewModel>();
            vm.Initialize();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void UpdateZone()
        {
            var vm = _globalScope.Resolve<ZoneViewModel>();
            vm.Initialize(SelectedZone);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void RemoveZone()
        {
            if (! await ConfirmZoneRemove()) return;

            var cmd = new RemoveZone() { ZoneId = SelectedZone.ZoneId };
            if (await _c2DWcfManager.SendCommandAsObj(cmd) == null)
            {
                var zone = SelectedZone;
                SelectedZone = Rows.First();
                Rows.Remove(zone);
            }
        }

        private async Task<bool> ConfirmZoneRemove()
        {
            var strs = new List<string>()
            {
                string.Format(Resources.SID_Zone___0___removal_, SelectedZone.Title),
                "",
                Resources.SID_Users_associated_with_this_zone_will_be_removed
            };
            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, strs, 0);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            return vm.IsAnswerPositive;
        }

        public async void Close()
        {
            await TryCloseAsync();
        }

    }
}
