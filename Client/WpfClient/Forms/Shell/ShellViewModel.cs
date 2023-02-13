using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IWindowManager _windowManager;
        private readonly LoginViewModel _loginViewModel;
        private readonly Heartbeater _heartbeater;
        private readonly ClientPoller _clientPoller;
        private readonly ClientGrpcServiceStarter _clientGrpcServiceStarter;
        private readonly ModelLoader _modelLoader;
        private readonly ILogger _logger;
        private readonly CurrentClientConfiguration _currentClientConfiguration;
        private readonly CurrentUser _currentUser;
        private readonly DataCenterConfig _currentDatacenterParameters;
        private readonly CommandLineParameters _commandLineParameters;
        private readonly GrpcC2DService? _grpcC2DService;
        private readonly ILifetimeScope _globalScope;
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly IWcfServiceInSuperClient _c2SWcfManager;

        public GraphReadModel GraphReadModel { get; set; }
        public MainMenuViewModel MainMenuViewModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; }
        public TabulatorViewModel TabulatorViewModel { get; }
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; }
        public OpticalEventsDoubleViewModel OpticalEventsDoubleViewModel { get; }
        public NetworkEventsDoubleViewModel NetworkEventsDoubleViewModel { get; }
        public BopNetworkEventsDoubleViewModel BopNetworkEventsDoubleViewModel { get; }

        public ShellViewModel(ILifetimeScope globalScope, IWritableConfig<ClientConfig> config, ILogger logger,
            CurrentClientConfiguration currentClientConfiguration, CurrentUser currentUser,
            DataCenterConfig currentDatacenterParameters, CommandLineParameters commandLineParameters,
            GrpcC2DService grpcC2DService,
            IWcfServiceInSuperClient c2SWcfManager,
            GraphReadModel graphReadModel, IWindowManager windowManager,
            LoginViewModel loginViewModel,
            Heartbeater heartbeater, ClientPoller clientPoller, ClientGrpcServiceStarter clientGrpcServiceStarter,
            MainMenuViewModel mainMenuViewModel, TreeOfRtuViewModel treeOfRtuViewModel,
            TabulatorViewModel tabulatorViewModel, CommonStatusBarViewModel commonStatusBarViewModel,
             OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
             NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
             BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            ModelLoader modelLoader
        )
        {
            GraphReadModel = graphReadModel;
            MainMenuViewModel = mainMenuViewModel;
            TreeOfRtuViewModel = treeOfRtuViewModel;
            TabulatorViewModel = tabulatorViewModel;
            CommonStatusBarViewModel = commonStatusBarViewModel;
            OpticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            NetworkEventsDoubleViewModel = networkEventsDoubleViewModel;
            BopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _globalScope = globalScope;
            _config = config;
            _c2SWcfManager = c2SWcfManager;
            _windowManager = windowManager;
            _loginViewModel = loginViewModel;
            _heartbeater = heartbeater;
            _clientPoller = clientPoller;
            _clientGrpcServiceStarter = clientGrpcServiceStarter;
            _modelLoader = modelLoader;
            _logger = logger;
            _currentClientConfiguration = currentClientConfiguration;
            _currentUser = currentUser;
            _currentDatacenterParameters = currentDatacenterParameters;
            _commandLineParameters = commandLineParameters;
            _grpcC2DService = grpcC2DService;
        }


        private readonly CancellationTokenSource _clientPollerCts = new CancellationTokenSource();
        private readonly CancellationTokenSource _heartbeaterCts = new CancellationTokenSource();

        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        private string _backgroundMessage = null!;
        public string BackgroundMessage
        {
            get => _backgroundMessage;
            set
            {
                if (value == _backgroundMessage) return;
                _backgroundMessage = value;
                NotifyOfPropertyChange();
            }
        }

        protected override async void OnViewLoaded(object view)
        {
            TabulatorViewModel.MessageVisibility = Visibility.Collapsed;
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            DisplayName = @"Fibertest v" + fvi.FileVersion;

            ((App)Application.Current).ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var postfix = _commandLineParameters.IsUnderSuperClientStart ? _commandLineParameters.ClientOrdinal.ToString() : "";
            // _logFile.AssignFile($@"client{postfix}.log");
            _logger.StartLine(Logs.Client);
            _logger.LogInfo(Logs.Client, $@"Client application {postfix} started!");

            if (_commandLineParameters.IsUnderSuperClientStart)
            {
                _config.Update(c => c.General.ServerAddress.Main = _commandLineParameters.ServerNetAddress!);
                _config.Update(c => c.General.ServerTitle = _commandLineParameters.ServerTitle!);
                _config.Update(c => c.General.Culture = _commandLineParameters.SuperClientCulture!);
                _config.Update(c => c.General.ClientOrdinal = _commandLineParameters.ClientOrdinal);
                await _loginViewModel.RegisterClientAsync(_commandLineParameters.Username!, _commandLineParameters.Password!,
                    _commandLineParameters.ConnectionId!, true, _commandLineParameters.ClientOrdinal);
            }
            else
                await _windowManager.ShowDialogWithAssignedOwner(_loginViewModel);

            ((App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;

            if (_loginViewModel.IsRegistrationSuccessful)
            {
                MainMenuViewModel.CurrentUser = _currentUser;
                TabulatorViewModel.SelectedTabIndex = 4;
                MainMenuViewModel.CurrentUser = _currentUser;

                SetDisplayName();
                StartSendHeartbeats();
                await GetAlreadyStoredOnServerData();
                StartRegularCommunicationWithServer();
                if (_commandLineParameters.IsUnderSuperClientStart)
                    await Task.Factory.StartNew(() => NotifySuperClientImReady(_commandLineParameters.ClientOrdinal));
                IsEnabled = true;
                _currentClientConfiguration.DoNotSignalAboutSuspicion =
                    _config.Value.Miscellaneous.DoNotSignalAboutSuspicion;
                TreeOfRtuViewModel.CollapseAll();
                TabulatorViewModel.SelectedTabIndex = 0; // the same value should be in TabulatorViewModel c-tor !!!
                var unused = await CheckFreeSpaceThreshold();
            }
            else
            {
                if (_commandLineParameters.IsUnderSuperClientStart)
                    await Task.Factory.StartNew(() => NotifySuperclientLoadingFailed(_commandLineParameters.ClientOrdinal));
                await TryCloseAsync();
            }
        }

        private void SetDisplayName()
        {
            const string separator = @"    >>    ";
            var server =
                $@"{separator}{_config.Value.General.ServerTitle} ({
                    _currentDatacenterParameters.General.ServerDoubleAddress.Main.Ip4Address}) v{
                        _currentDatacenterParameters.General.DatacenterVersion}";
            var user = $@"{separator}{_currentUser.UserName} ({_currentUser.Role.ToString()})";
            var zone = $@"{separator}[{_currentUser.ZoneTitle}]";
            DisplayName += $@" {server} {user} {zone}";
        }

        private async Task<bool> CheckFreeSpaceThreshold()
        {
            var dto = new GetDiskSpaceDto();
            var driveInfo = await _grpcC2DService!.SendAnyC2DRequest<GetDiskSpaceDto, DiskSpaceDto>(dto);
            var totalSize = $@"Database drive's size: {driveInfo.TotalSize:0.0}Gb";
            var freeSpace = $@"free space: {driveInfo.AvailableFreeSpace:0.0}Gb";
            var dataSize = $@"database size: {driveInfo.DataSize:0.0}Gb";
            var threshold = $@"threshold: {driveInfo.FreeSpaceThreshold:0.0}Gb";
            _logger.LogInfo(Logs.Client, $@"{totalSize},  {dataSize},  {freeSpace},  {threshold}");
            if (driveInfo.AvailableFreeSpace < driveInfo.FreeSpaceThreshold)
            {
                var str = new List<string>()
                {
                    Resources.SID_Free_space_threshold_exceeded_, "",
                    $@"{Resources.SID_Db_drive_free_space}  {driveInfo.AvailableFreeSpace:0.0} Gb",
                    $@"{Resources.SID_Free_space_threshold}  {driveInfo.FreeSpaceThreshold:0.0} Gb", "",
                    $@"{Resources.SID_Db_drive_total_size}  {driveInfo.TotalSize:0.0} Gb",
                    $@"{Resources.SID_Fibertest_data_size}  {driveInfo.DataSize:0.000} Gb",
                };
                var vm = new MyMessageBoxViewModel(MessageType.Information, str, 2);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }

            return true;
        }

        private async Task NotifySuperClientImReady(int postfix)
        {
            _logger.LogInfo(Logs.Client, @"Notify super-client I'm ready");
            Thread.Sleep(TimeSpan.FromMilliseconds(1));
            var isStateOk = !OpticalEventsDoubleViewModel.ActualOpticalEventsViewModel.Rows.Any() &&
                            !NetworkEventsDoubleViewModel.ActualNetworkEventsViewModel.Rows.Any() &&
                            !BopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows.Any();
            await _c2SWcfManager.ClientLoadingResult(postfix, true, isStateOk);
        }

        private async Task NotifySuperclientLoadingFailed(int postfix)
        {
            await _c2SWcfManager.ClientLoadingResult(postfix, false, false);
        }

        public async Task GetAlreadyStoredOnServerData()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                BackgroundMessage = Resources.SID_Data_is_loading;
                var res = await _modelLoader.DownloadAndApplyModel();
                _clientPoller.CurrentEventNumber = res;
            }
        }

        private void StartSendHeartbeats()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                _heartbeater.Start(_heartbeaterCts.Token);
            }
        }

        private void StartRegularCommunicationWithServer()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                _clientPoller.Start(_clientPollerCts.Token); // graph events including monitoring results events

                // Accepts only monitoring step messages and client's measurement results
                Task.Factory.StartNew(() => _clientGrpcServiceStarter.StartClientGrpcListener());
            }
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (_loginViewModel.IsRegistrationSuccessful)
            {
                var question = Resources.SID_Close_application_;
                var vm = new MyMessageBoxViewModel(MessageType.Confirmation, question);
                await _windowManager.ShowDialogWithAssignedOwner(vm);

                if (!vm.IsAnswerPositive) return false;
            }

            _heartbeaterCts.Cancel();
            _clientPollerCts.Cancel();
            _logger.LogInfo(Logs.Client, @"Client application finished.");

            if (_grpcC2DService == null)
                return true;

            await _grpcC2DService.SendAnyC2DRequest<UnRegisterClientDto, RequestAnswer>(new UnRegisterClientDto(_currentUser.UserName))
                .ContinueWith(_ => { Environment.Exit(Environment.ExitCode); });

            return true;
        }
    }
}