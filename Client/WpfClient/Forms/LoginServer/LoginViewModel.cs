using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class LoginViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IMachineKeyProvider _machineKeyProvider;
        private readonly NoLicenseAppliedViewModel _noLicenseAppliedViewModel;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly GrpcC2RService _grpcC2RService;
        private readonly SecurityAdminConfirmationViewModel _securityAdminConfirmationViewModel;
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly ILogger _logger; 
        private readonly CurrentUser _currentUser;
        private readonly CurrentGis _currentGis;
        private readonly DataCenterConfig _currentDatacenterParameters;

        private string _userName = "";
        public string UserName
        {
            get => _userName;
            set { _userName = value; Status = Resources.SID_Input_user_name_and_password; }
        }

        private bool _isButtonEnabled;
        public bool IsButtonEnabled
        {
            get => _isButtonEnabled;
            set
            {
                if (value == _isButtonEnabled) return;
                _isButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public PasswordViewModel PasswordViewModel { get; set; } = new PasswordViewModel();

        public string ConnectionId { get; set; } = null!;

        private string _status = Resources.SID_Input_user_name_and_password;
        public string Status
        {
            get => _status;
            set
            {
                if (value == _status) return;
                _status = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsRegistrationSuccessful { get; set; }

        public LoginViewModel(ILifetimeScope globalScope, IWritableConfig<ClientConfig> config, ILogger logger,
            IWindowManager windowManager, SecurityAdminConfirmationViewModel securityAdminConfirmationViewModel,
            IMachineKeyProvider machineKeyProvider, NoLicenseAppliedViewModel noLicenseAppliedViewModel,
            GrpcC2DService grpcC2DService, GrpcC2RService grpcC2RService,
            CurrentUser currentUser, CurrentGis currentGis, DataCenterConfig currentDatacenterParameters)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _machineKeyProvider = machineKeyProvider;
            _noLicenseAppliedViewModel = noLicenseAppliedViewModel;
            _grpcC2DService = grpcC2DService;
            _grpcC2RService = grpcC2RService;
            _securityAdminConfirmationViewModel = securityAdminConfirmationViewModel;
            _config = config;
            _logger = logger;
            _currentUser = currentUser;
            _currentGis = currentGis;
            _currentDatacenterParameters = currentDatacenterParameters;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Authentication;
            IsButtonEnabled = true;
        }

        public async void Login()
        {
            IsButtonEnabled = false;
#if DEBUG
            if (string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(PasswordViewModel.Password))
            {
                // UserName = @"superclient";  PasswordViewModel.Password = @"superclient";
                UserName = @"developer"; PasswordViewModel.Password = @"developer";
                // UserName = @"12"; PasswordViewModel.Password = @"12";
                // UserName = @"operator"; PasswordViewModel.Password = @"operator";
                // UserName = @"supervisor"; PasswordViewModel.Password = @"supervisor";
                // UserName = @"root"; PasswordViewModel.Password = @"root";
            }
#endif
            if (string.IsNullOrEmpty(ConnectionId))
                ConnectionId = Guid.NewGuid().ToString();

            var unused = await RegisterClientAsync(UserName, PasswordViewModel.Password, ConnectionId);
            IsButtonEnabled = true;
        }

        private NetAddress _clientAddress = null!;

        // public to start under super-client
        public async Task<bool> RegisterClientAsync(string username, string password,
            string connectionId, bool isUnderSuperClient = false, int ordinal = 0)
        {
            PrepareAddresses(isUnderSuperClient, ordinal);
          
            var sendDto = new RegisterClientDto(username, password.GetHashString()!)
            {
                ClientConnectionId = connectionId,
                ClientIp = _clientAddress.Ip4Address,
                MachineKey = _machineKeyProvider.Get(),
                IsUnderSuperClient = isUnderSuperClient,
                Addresses = new DoubleAddress() { Main = _clientAddress, HasReserveAddress = false }
            };

            return await ProcessRegistrationResult(sendDto);
        }

        private void PrepareAddresses(bool isUnderSuperClient = false, int ordinal = 0)
        {
            var grpcIp = _config.Value.General.ServerAddress.Main.Ip4Address;

            _currentDatacenterParameters.General.ServerDoubleAddress = _config.Value.General.ServerAddress;
            _currentDatacenterParameters.General.ServerTitle = _config.Value.General.ServerTitle;

            var clientTcpPort = (int)TcpPorts.ClientListenTo;
            if (isUnderSuperClient)
                clientTcpPort += ordinal;

            _clientAddress = _config.Value.General.ClientLocalAddress;
            _clientAddress.Port = clientTcpPort;

            if (_clientAddress.IsAddressSetAsIp && _clientAddress.Ip4Address == @"0.0.0.0" &&
                grpcIp != @"0.0.0.0")
            {
                _clientAddress.Ip4Address = LocalAddressResearcher.GetLocalAddressToConnectServer(grpcIp) ?? "0.0.0.0";
                _config.Update(c=>c.General.ClientLocalAddress = _clientAddress);
            }

            _grpcC2DService.SetClientConnectionId(ConnectionId);
            //_grpcC2DService.ChangeAddress(grpcIp);
            _grpcC2RService.SetClientConnectionId(ConnectionId);
            Status = string.Format(Resources.SID_Performing_registration_on__0_, grpcIp);
            _logger.LogInfo(Logs.Client,$@"Performing registration on {grpcIp}");
        }

        private async Task<bool> ProcessRegistrationResult(RegisterClientDto sendDto)
        {
            var resultDto = await SendAsync(sendDto);
            if (resultDto.ReturnCode == ReturnCode.NoLicenseHasBeenAppliedYet)
            {
                _currentUser.UserName = UserName; // it will be sent with license
                await _windowManager.ShowDialogWithAssignedOwner(_noLicenseAppliedViewModel);
                if (!_noLicenseAppliedViewModel.IsCommandSent)
                    return false;
                if (!_noLicenseAppliedViewModel.IsLicenseAppliedSuccessfully)
                    return false; // message was shown already

                sendDto.SecurityAdminPassword = _noLicenseAppliedViewModel.SecurityAdminPassword.GetHashString();
                resultDto = await SendAsync(sendDto);
            }
            else if (resultDto.ReturnCode == ReturnCode.WrongMachineKey
                     || resultDto.ReturnCode == ReturnCode.EmptyMachineKey
                     || resultDto.ReturnCode == ReturnCode.WrongSecurityAdminPassword)
            {
                if (! await AskSecurityAdminPassword(resultDto))
                   return false;

                sendDto.SecurityAdminPassword = _securityAdminConfirmationViewModel.PasswordViewModel.Password.GetHashString();
                resultDto = await SendAsync(sendDto);
            }

            if (resultDto.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, resultDto.ReturnCode.GetLocalizedString());
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }

            ParseServerAnswer(resultDto);
            return true;
        }

        private async Task<ClientRegisteredDto> SendAsync(RegisterClientDto dto)
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                return await _grpcC2DService.SendAnyC2DRequest<RegisterClientDto, ClientRegisteredDto>(dto);
            }
        }

        private async Task<bool> AskSecurityAdminPassword(ClientRegisteredDto resultDto)
        {
            _securityAdminConfirmationViewModel.Initialize(resultDto);
            await _windowManager.ShowDialogWithAssignedOwner(_securityAdminConfirmationViewModel);
            return _securityAdminConfirmationViewModel.IsOkPressed;
        }

        public async void ChooseServer()
        {
            var vm = _globalScope.Resolve<ServersConnectViewModel>();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        private async void ParseServerAnswer(ClientRegisteredDto result)
        {
            if (result.ReturnCode == ReturnCode.ClientRegisteredSuccessfully)
            {
                _currentUser.FillIn(result);
                _currentUser.UserName = UserName;

                _currentDatacenterParameters.FillIn(result.DcCurrentParameters);

                _currentGis.IsWithoutMapMode = result.DcCurrentParameters.General.IsWithoutMapMode;

                _logger.LogInfo(Logs.Client,
                    $@"Successfully registered on server {_currentDatacenterParameters.General.ServerTitle}");
                _logger.LogInfo(Logs.Client,
                    $@"StreamIdOriginal = {result.DcCurrentParameters.EventSourcing.StreamIdOriginal
                    }  Last event number in snapshot {result.DcCurrentParameters.EventSourcing.SnapshotLastEvent}");
                IsRegistrationSuccessful = true;
                await TryCloseAsync();
            }
            else
            {
                _logger.LogError(Logs.Client,result.ReturnCode.ToString());
                Status = result.ReturnCode.GetLocalizedWithOsInfo(result.ErrorMessage);
            }
        }

        public async void Cancel()
        {
            await TryCloseAsync();
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (!IsRegistrationSuccessful)
            {
                var question = Resources.SID_Close_application_;
                var vm = new MyMessageBoxViewModel(MessageType.Confirmation, question);
                await _windowManager.ShowDialogWithAssignedOwner(vm);

                if (!vm.IsAnswerPositive) return true;
            }
            return true;
        }
    }
}
