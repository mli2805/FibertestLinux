using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.Utils.Setup;
using Fibertest.WpfCommonViews;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace KadastrLoader
{
    public class KadastrLoaderViewModel : Screen, IShell
    {
        private ILogger _logger;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly ILifetimeScope _globalScope;
        private readonly LoadedAlready _loadedAlready;
        private readonly KadastrDbProvider _kadastrDbProvider;
        private readonly KadastrFilesParser _kadastrFilesParser;
        public string ServerIp { get; set; }
        public int MySqlPort { get; set; }

        private string _kadastrMessage = "";
        public string KadastrMessage
        {
            get { return _kadastrMessage; }
            set
            {
                if (value == _kadastrMessage) return;
                _kadastrMessage = value;
                NotifyOfPropertyChange();
            }
        }

        private string _serverMessage = "";
        public string ServerMessage
        {
            get => _serverMessage;
            set
            {
                if (value == _serverMessage) return;
                _serverMessage = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsStartEnabled => _isDbReady && _isDataCenterReady && _isFolderValid;

        private string _selectedFolder = "";
        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (value == _selectedFolder) return;
                _selectedFolder = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsFree
        {
            get { return _isFree; }
            set
            {
                if (value == _isFree) return;
                _isFree = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<string> ProgressLines { get; set; } = new ObservableCollection<string>();

        public KadastrLoaderViewModel(IWritableConfig<ClientConfig> config, ILogger logger,
            GrpcC2DService grpcC2DService, ILifetimeScope globalScope,
            LoadedAlready loadedAlready, KadastrDbProvider kadastrDbProvider,
            KadastrFilesParser kadastrFilesParser)
        {
            _logger = logger;
            _logger.LogInfo(Logs.Client, "We are in c-tor");
            _grpcC2DService = grpcC2DService;
            _globalScope = globalScope;
            ServerIp = config.Value.General.ServerAddress.Main.Ip4Address;
            _loadedAlready = loadedAlready;
            _kadastrDbProvider = kadastrDbProvider;
            _kadastrFilesParser = kadastrFilesParser;
            MySqlPort = config.Value.General.MysqlTcpPort;
            IsFree = true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Load_from_Kadastr;
        }

        private bool _isDbReady;
        private bool _isDataCenterReady;

        public async void CheckConnect()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                _isDataCenterReady = await RegisterClientOnDataCenter();
                _isDbReady = await ConnectKadastrDb();
                NotifyOfPropertyChange(nameof(IsStartEnabled));
            }

        }
        private async Task<bool> ConnectKadastrDb()
        {
            try
            {
                _kadastrDbProvider.Init();
                _loadedAlready.Wells = await _kadastrDbProvider.GetWells();
                _loadedAlready.Conpoints = await _kadastrDbProvider.GetConpoints();
                var count = _loadedAlready.Wells.Count;
                KadastrMessage = string.Format(Resources.SID_Nodes_loaded_from_Kadastr_so_far___0_, count);
                NotifyOfPropertyChange(nameof(IsStartEnabled));

                return true;
            }
            catch (Exception e)
            {
                KadastrMessage = Resources.SID_Kadastr_Db_connection_error___ + e.Message;
                return false;
            }
        }

        private ClientRegisteredDto? _clientRegisteredDto;
        private async Task<bool> RegisterClientOnDataCenter()
        {
            _clientRegisteredDto = await _grpcC2DService
                .SendAnyC2DRequest<RegisterClientDto, ClientRegisteredDto>(new RegisterClientDto("developer", "developer".GetSha256()));
            var isRegistered = _clientRegisteredDto.ReturnCode == ReturnCode.ClientRegisteredSuccessfully;
            if (isRegistered)
                _grpcC2DService.SetClientConnectionId(_clientRegisteredDto.ConnectionId!);
            ServerMessage = isRegistered ? Resources.SID_DataCenter_connected_successfully_ : Resources.SID_DataCenter_connection_failed_;
            _logger.LogInfo(Logs.Client, ServerMessage);
            return isRegistered;
        }

        private bool _isFolderValid;
        private bool _isFree;

        public void SelectFolder()
        {
            var dlg = new OpenFileDialog
            {
                InitialDirectory = FileOperations.GetParentFolder(FileOperations.GetMainFolder() + @"\any.file"),
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            };
            if (dlg.ShowDialog() == true)
            {
                SelectedFolder = Path.GetDirectoryName(dlg.FileName)!;
                _isFolderValid = true;
                NotifyOfPropertyChange(nameof(IsStartEnabled));
            }
        }

        public void Start()
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += Bw_DoWork;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;

            IsFree = false;
            Mouse.OverrideCursor = Cursors.Wait;
            ProgressLines.Clear();
            bw.RunWorkerAsync();
        }

        private void Bw_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            ProgressLines.Add(Resources.SID_Done_);
            Mouse.OverrideCursor = null;
            IsFree = true;
        }

        private void Bw_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            var st = (string?)e.UserState;
            if (st != null)
                ProgressLines.Add(st);
        }

        private void Bw_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (sender is BackgroundWorker worker)
                _kadastrFilesParser.Run(SelectedFolder, worker);
        }

        public async void CloseButton()
        {
            await TryCloseAsync();
        }
    }
}