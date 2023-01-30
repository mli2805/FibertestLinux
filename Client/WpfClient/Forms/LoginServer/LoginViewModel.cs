using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using GrpsClientLib;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class LoginViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IMachineKeyProvider _machineKeyProvider;
        private readonly NoLicenseAppliedViewModel _noLicenseAppliedViewModel;
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly SecurityAdminConfirmationViewModel _securityAdminConfirmationViewModel;
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly ILogger _logger; 
        private readonly IWcfServiceDesktopC2D _desktopC2DWcfManager;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
        private readonly CurrentUser _currentUser;
        private readonly CurrentGis _currentGis;
        private readonly DataCenterConfig _currentDatacenterParameters;

        private string _userName;
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

        public string ConnectionId { get; set; }

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
            GrpcC2DRequests grpcC2DRequests,
            IWcfServiceDesktopC2D desktopC2DWcfManager, IWcfServiceCommonC2D commonC2DWcfManager,
            CurrentUser currentUser, CurrentGis currentGis, DataCenterConfig currentDatacenterParameters)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _machineKeyProvider = machineKeyProvider;
            _noLicenseAppliedViewModel = noLicenseAppliedViewModel;
            _grpcC2DRequests = grpcC2DRequests;
            _securityAdminConfirmationViewModel = securityAdminConfirmationViewModel;
            _config = config;
            _logger = logger;
            _desktopC2DWcfManager = desktopC2DWcfManager;
            _commonC2DWcfManager = commonC2DWcfManager;
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

        private DoubleAddress _commonServiceAddresses;
        private DoubleAddress _desktopServiceAddresses;
        private NetAddress _clientAddress;
        private RegisterClientDto _sendDto;

        private void PrepareAddresses(string username, bool isUnderSuperClient = false, int ordinal = 0)
        {
            _desktopServiceAddresses = _config.Value.General.ServerAddress;
            _currentDatacenterParameters.General.ServerDoubleAddress.Main.Ip4Address =
                _desktopServiceAddresses.Main.Ip4Address;
            _currentDatacenterParameters.General.ServerTitle = _config.Value.General.ServerTitle;

            var clientTcpPort = (int)TcpPorts.ClientListenTo;
            if (isUnderSuperClient)
                clientTcpPort += ordinal;
            _clientAddress = _config.Value.General.ClientLocalAddress;
            _clientAddress.Port = clientTcpPort;

            if (_clientAddress.IsAddressSetAsIp && _clientAddress.Ip4Address == @"0.0.0.0" &&
                _desktopServiceAddresses.Main.Ip4Address != @"0.0.0.0")
            {
                _clientAddress.Ip4Address = LocalAddressResearcher.GetLocalAddressToConnectServer(_desktopServiceAddresses.Main.Ip4Address);
                _config.Update(c=>c.General.ClientLocalAddress = _clientAddress);
            }

            _commonServiceAddresses = (DoubleAddress)_desktopServiceAddresses.Clone();
            _commonServiceAddresses.Main.Port = (int)TcpPorts.ServerListenToCommonClient;
            if (_commonServiceAddresses.HasReserveAddress)
                _commonServiceAddresses.Reserve.Port = (int)TcpPorts.ServerListenToCommonClient;

            _desktopC2DWcfManager.SetServerAddresses(_desktopServiceAddresses, username, _clientAddress.Ip4Address);
            _commonC2DWcfManager.SetServerAddresses(_commonServiceAddresses, username, _clientAddress.Ip4Address);
        }

        // public to start under super-client
        public async Task<bool> RegisterClientAsync(string username, string password,
            string connectionId, bool isUnderSuperClient = false, int ordinal = 0)
        {
            PrepareAddresses(username, isUnderSuperClient, ordinal);

            Status = string.Format(Resources.SID_Performing_registration_on__0_, _desktopServiceAddresses.Main.Ip4Address);
            _logger.LogInfo(Logs.Client,$@"Performing registration on {_desktopServiceAddresses.Main.Ip4Address}");

            _sendDto = new RegisterClientDto(username, password.GetHashString()!)
            {
                ClientConnectionId = connectionId,
                MachineKey = _machineKeyProvider.Get(),
                IsUnderSuperClient = isUnderSuperClient,
                Addresses = new DoubleAddress() { Main = _clientAddress, HasReserveAddress = false }
            };

            return await ProcessRegistrationResult();
        }

        private async Task<bool> ProcessRegistrationResult()
        {
            var resultDto = await SendAsync(_sendDto);
            if (resultDto.ReturnCode == ReturnCode.NoLicenseHasBeenAppliedYet)
            {
                _windowManager.ShowDialogWithAssignedOwner(_noLicenseAppliedViewModel);
                if (!_noLicenseAppliedViewModel.IsCommandSent)
                    return false;
                if (!_noLicenseAppliedViewModel.IsLicenseAppliedSuccessfully)
                    return false; // message was shown already

                _sendDto.SecurityAdminPassword = _noLicenseAppliedViewModel.SecurityAdminPassword.GetHashString();
                resultDto = await SendAsync(_sendDto);
            }
            else if (resultDto.ReturnCode == ReturnCode.WrongMachineKey
                     || resultDto.ReturnCode == ReturnCode.EmptyMachineKey
                     || resultDto.ReturnCode == ReturnCode.WrongSecurityAdminPassword)
            {
                if (!AskSecurityAdminPassword(resultDto))
                   return false;

                _sendDto.SecurityAdminPassword = _securityAdminConfirmationViewModel.PasswordViewModel.Password.GetHashString();
                resultDto = await SendAsync(_sendDto);
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
                return await _grpcC2DRequests.RegisterClient(dto);
            }
        }

        private bool AskSecurityAdminPassword(ClientRegisteredDto resultDto)
        {
            _securityAdminConfirmationViewModel.Initialize(resultDto);
            _windowManager.ShowDialogWithAssignedOwner(_securityAdminConfirmationViewModel);
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
