using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class DbOptimizationViewModel : Screen
    {
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly ILogger<DbOptimizationViewModel> _logger; 
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public bool IsInProgress { get; set; }

        public DbOptimizationModel Model { get; set; } = new DbOptimizationModel();

        public DbOptimizationViewModel(IWritableConfig<ClientConfig> config, ILogger<DbOptimizationViewModel> logger, Model readModel, 
            CurrentUser currentUser, 
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager)
        {
            _config = config;
            _logger = logger;
            _readModel = readModel;
            _currentUser = currentUser;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
        }

        public async Task Initialize()
        {
            var drive = await _c2DWcfManager.GetDiskSpaceGb();
            if (drive == null)
            {
                _logger.LogError(Logs.Client,@"GetDiskSpaceGb error");
                return;
            }
            Model.DriveSize = $@"{drive.TotalSize:0.0} Gb";
            Model.DataSize = $@"{drive.DataSize:0.000} Gb";
            Model.AvailableFreeSpace = $@"{drive.AvailableFreeSpace:0.0} Gb";
            Model.FreeSpaceThreshold = $@"{drive.FreeSpaceThreshold:0.0} Gb";

            Model.OpticalEvents = _readModel.Measurements.Count(m => m.EventStatus > EventStatus.JustMeasurementNotAnEvent);
            Model.MeasurementsNotEvents = _readModel.Measurements.Count(m => m.EventStatus == EventStatus.JustMeasurementNotAnEvent);
            Model.NetworkEvents = _readModel.NetworkEvents.Count + _readModel.BopNetworkEvents.Count;

            var flag = _config.Value.Miscellaneous.CouldUserDoOptimizationUpToToday;
            Model.UpToLimit = flag ? DateTime.Today.AddDays(-1) : new DateTime(DateTime.Today.Year - 2, 12, 31);
            Model.SelectedDate = flag ? DateTime.Today.AddDays(-1) : new DateTime(DateTime.Today.Year - 2, 12, 31);

            Model.IsEnabled = _currentUser.Role <= Role.Root;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Database_optimization;
        }

        public async void Execute()
        {
            if (! await Validate()) return;

            IsInProgress = true;

            var cmd = Model.IsRemoveMode ?
                (object)new RemoveEventsAndSors()
                {
                    IsMeasurementsNotEvents = Model.IsMeasurements,
                    IsOpticalEvents = Model.IsOpticalEvents,
                    IsNetworkEvents = Model.IsNetworkEvents,
                    UpTo = Model.SelectedDate,
                }
                : new MakeSnapshot()
                {
                    UpTo = DateTime.Today,
                };
            var result = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (!string.IsNullOrEmpty(result))
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_DB_optimization__ + result);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }

            IsInProgress = false;
        }

        private async Task<bool> Validate()
        {
            if (Model.IsRemoveMode && !Model.IsMeasurements && !Model.IsOpticalEvents && !Model.IsNetworkEvents)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Nothing_selected_to_remove_);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }

            var vm2 = new MyMessageBoxViewModel(MessageType.Confirmation,
                new List<string>
                {
                    Resources.SID_Attention_, "",
                    Resources.SID_If_you_click_OK_now__the_data_will_be_permanently_deleted,
                    Resources.SID_with_no_possibility_to_restore_them_,
                }, 0);
            await _windowManager.ShowDialogWithAssignedOwner(vm2);
            return vm2.IsAnswerPositive;
        }

    }
}
