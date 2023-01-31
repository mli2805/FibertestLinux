using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.StringResources;
using Fibertest.Utils;

namespace Fibertest.WpfClient
{
    public class ServersConnectViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWritableConfig<ClientConfig> _config;
        public string NewServerTitle { get; set; } = "";

        public Visibility NewServerTitleVisibility
        {
            get => _newServerTitleVisibility;
            set
            {
                if (value == _newServerTitleVisibility) return;
                _newServerTitleVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<ServerForClient> Servers { get; set; } = null!;

        public Visibility ServersComboboxVisibility
        {
            get => _serversComboboxVisibility;
            set
            {
                if (value == _serversComboboxVisibility) return;
                _serversComboboxVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsRemoveServerEnabled
        {
            get => _isRemoveServerEnabled;
            set
            {
                if (value == _isRemoveServerEnabled) return;
                _isRemoveServerEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        private ServerForClient? _selectedServerForClient;
        public ServerForClient? SelectedServerForClient
        {
            get => _selectedServerForClient;
            set
            {
                if (Equals(value, _selectedServerForClient)) return;
                _selectedServerForClient = value;
                NotifyOfPropertyChange();
                if (_selectedServerForClient != null)
                    ToggleToSelectServerMode();
            }
        }

        private string _clientAddress = "";

        private NetAddressTestViewModel _serverConnectionTestViewModel = null!;
        public NetAddressTestViewModel ServerConnectionTestViewModel
        {
            get => _serverConnectionTestViewModel;
            set
            {
                if (Equals(value, _serverConnectionTestViewModel)) return;
                _serverConnectionTestViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        private string _message = "";
        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                NotifyOfPropertyChange();
            }
        }

        public ServersConnectViewModel(ILifetimeScope globalScope, IWritableConfig<ClientConfig> config)
        {
            _globalScope = globalScope;
            _config = config;
            InitializeView();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Server;
            Message = Resources.SID_Enter_DataCenter_Server_address;
        }

        private void InitializeView()
        {
            Servers = new ObservableCollection<ServerForClient>();
            _config.Value.ServerList.ForEach(s => Servers.Add(s));
            SelectedServerForClient = Servers.FirstOrDefault(s => s.IsLastSelected) ?? Servers.FirstOrDefault();
            IsRemoveServerEnabled = Servers.Count > 0;
            if (SelectedServerForClient == null)
                ToggleToAddServerMode();
            else
                ToggleToSelectServerMode();
        }

        private void ServerConnectionTestViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Result")
            {
                if (ServerConnectionTestViewModel.Result == true)
                {
                    var netAddress = ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress();
                    var serverAddress = netAddress.IsAddressSetAsIp ? netAddress.Ip4Address : netAddress.HostName;
                    if (serverAddress == @"localhost")
                    {
                        var serverIp = LocalAddressResearcher.GetAllLocalAddresses().First();
                        ServerConnectionTestViewModel.NetAddressInputViewModel =
                            new NetAddressInputViewModel(new NetAddress(serverIp, TcpPorts.ServerListenToDesktopClient), true);
                        serverAddress = serverIp;
                    }
                    _clientAddress = LocalAddressResearcher.GetLocalAddressToConnectServer(serverAddress) ?? "";
                    Message = Resources.SID_Connection_established_successfully_
                              + System.Environment.NewLine + string.Format(Resources.SID___Client_s_address__0_, _clientAddress);
                }
                else
                    Message = ServerConnectionTestViewModel.Result == null
                        ? Resources.SID_Establishing_connection___
                        : Resources.SID_Cant_establish_connection_;
            }
        }

        private bool _isInAddMode;
        private readonly NetAddress _serverInWorkAddress = new NetAddress(@"0.0.0.0", TcpPorts.ServerListenToCommonClient);
        private Visibility _serversComboboxVisibility = Visibility.Visible;
        private Visibility _newServerTitleVisibility = Visibility.Collapsed;
        private bool _isRemoveServerEnabled;

        public void ButtonPlus()
        {
            if (!_isInAddMode)
                ToggleToAddServerMode();
        }

        public void ButtonMinus()
        {
            Servers.Remove(SelectedServerForClient!);
            SaveChanges();

            if (Servers.Count > 0)
                SelectedServerForClient = Servers.First(s=>!s.Equals(SelectedServerForClient));
            else
                ToggleToAddServerMode();

            IsRemoveServerEnabled = Servers.Count > 0;
        }

        private void ToggleToAddServerMode()
        {
            ServersComboboxVisibility = Visibility.Collapsed;

            ServerConnectionTestViewModel = _globalScope.Resolve<NetAddressTestViewModel>
                (new NamedParameter(@"netAddressForConnectionTest", new NetAddressForConnectionTest(_serverInWorkAddress.Clone(), false)));
            ServerConnectionTestViewModel.PropertyChanged += ServerConnectionTestViewModel_PropertyChanged;

            NewServerTitleVisibility = Visibility.Visible;
            _isInAddMode = true;
        }

        private void ToggleToSelectServerMode()
        {
            ServersComboboxVisibility = Visibility.Visible;

            var address = SelectedServerForClient?.ServerAddress.Main.Clone() ?? new NetAddress(@"0.0.0.0", 11840);
            ServerConnectionTestViewModel = _globalScope.Resolve<NetAddressTestViewModel>
                (new NamedParameter(@"netAddressForConnectionTest", new NetAddressForConnectionTest(address, false)));
            ServerConnectionTestViewModel.PropertyChanged += ServerConnectionTestViewModel_PropertyChanged;

            NewServerTitleVisibility = Visibility.Collapsed;
            _isInAddMode = false;
        }

        public async void Cancel()
        {
            if (!_isInAddMode) await TryCloseAsync();

            ToggleToSelectServerMode();
        }

        private void AddServerIntoList()
        {
            var newServer = new ServerForClient()
            {
                Title = NewServerTitle,
                ServerAddress = new DoubleAddress()
                {
                    Main = ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress().Clone()
                },
                IsLastSelected = true,
            };

            Servers.Add(newServer);
            SelectedServerForClient = newServer;
        }

        public async void Save()
        {
            if (_isInAddMode)
                AddServerIntoList();

            SaveChanges();
            await TryCloseAsync(true);
        }

        private void SaveChanges()
        {
            var serversList = Servers.ToList();
            serversList.ForEach(s => s.IsLastSelected = s.Equals(SelectedServerForClient));

            if (SelectedServerForClient != null)
            {
                SelectedServerForClient.ServerAddress.Main = ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress().Clone();
                _config.Update(c=>c.General.ServerAddress = SelectedServerForClient.ServerAddress);
                _config.Update(c=>c.General.ServerTitle = SelectedServerForClient.Title);
            }

            var clientAddress = new NetAddress(_clientAddress, TcpPorts.ClientListenTo);
            _config.Update(c=>c.General.ClientLocalAddress = clientAddress);

            _config.Update(c=>c.ServerList = serversList);
        }
    }
}
