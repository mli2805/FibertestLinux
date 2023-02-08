using System.Collections.Generic;
using System.Linq;
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
    public class ClientMeasurementViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly ILogger _logger;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly MeasurementInterrupter _measurementInterrupter;
        private readonly GrpcC2RService _grpcC2RService;
        private readonly IWindowManager _windowManager;
        private readonly VeexMeasurementTool _veexMeasurementTool;
        private readonly ReflectogramManager _reflectogramManager;
        public RtuLeaf RtuLeaf { get; set; }
        private Rtu _rtu;
        private DoClientMeasurementDto _dto;
        private OtdrParametersThroughServerSetterViewModel _vm;

        public bool IsOpen { get; set; }

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

        private bool _isCancelButtonEnabled;
        public bool IsCancelButtonEnabled
        {
            get => _isCancelButtonEnabled;
            set
            {
                if (value == _isCancelButtonEnabled) return;
                _isCancelButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public ClientMeasurementViewModel(ILifetimeScope globalScope, 
            ILogger logger,
            CurrentUser currentUser, Model readModel, MeasurementInterrupter measurementInterrupter,
            GrpcC2RService grpcC2RService, IWindowManager windowManager,
            VeexMeasurementTool veexMeasurementTool,
            ReflectogramManager reflectogramManager)
        {
            _globalScope = globalScope;
            _logger = logger;
            _currentUser = currentUser;
            _readModel = readModel;
            _measurementInterrupter = measurementInterrupter;
            _grpcC2RService = grpcC2RService;
            _windowManager = windowManager;
            _veexMeasurementTool = veexMeasurementTool;
            _reflectogramManager = reflectogramManager;
        }

        public async Task<bool> Initialize(Leaf parent, int portNumber)
        {
            RtuLeaf = parent as RtuLeaf ?? (RtuLeaf)parent.Parent;
            _rtu = _readModel.Rtus.First(r => r.Id == RtuLeaf.Id);

            _vm = _globalScope.Resolve<OtdrParametersThroughServerSetterViewModel>();
            _vm.Initialize(_rtu.AcceptableMeasParams);
            IWindowManager windowManager = new WindowManager();
            await windowManager.ShowDialogWithAssignedOwner(_vm);
            if (!_vm.IsAnswerPositive)
                return false;

            _dto = parent
                .CreateDoClientMeasurementDto(portNumber, false, _readModel, _currentUser)
                .SetParams(false, false, false, _vm.GetSelectedParameters(), _vm.GetVeexSelectedParameters());

            return true;
        }

        public DoClientMeasurementDto ForUnitTests(Leaf parent, int portNumber,
            List<MeasParamByPosition> iitMeasParams, VeexMeasOtdrParameters veexMeasParams)
        {
            return parent
                .CreateDoClientMeasurementDto(portNumber, false, _readModel, _currentUser)
                .SetParams(false, false, false, iitMeasParams, veexMeasParams);
        }

        private CancellationTokenSource _cts;
        protected override async void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Measurement__Client_;
            IsOpen = true;
            IsCancelButtonEnabled = false;

            Message = Resources.SID_Sending_command__Wait_please___;
            var startResult =
                await _grpcC2RService.SendAnyC2RRequest<DoClientMeasurementDto, ClientMeasurementStartedDto>(_dto);
            if (startResult.ReturnCode != ReturnCode.MeasurementClientStartedSuccessfully)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, startResult.ErrorMessage ?? "");
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                await TryCloseAsync();
                return;
            }

            Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            IsCancelButtonEnabled = true;

            if (_rtu.RtuMaker == RtuMaker.VeEX)
            {
                _cts = new CancellationTokenSource();
                var veexResult = await _veexMeasurementTool.Fetch(_dto.RtuId, null, startResult.ClientMeasurementId, _cts);
                if (veexResult.Code == ReturnCode.MeasurementEndedNormally)
                    ShowReflectogram(veexResult.SorBytes);
                await TryCloseAsync(true);
            }
        }

        public void ShowResult(ClientMeasurementResultDto dto)
        {
            if (dto.ReturnCode == ReturnCode.MeasurementInterrupted)
                _logger.LogInfo(Logs.Client, @"Measurement interrupted");
            else
                ShowReflectogram(dto.SorBytes!);
        }

        private async void ShowReflectogram(byte[] sorBytes)
        {
            _reflectogramManager.ShowClientMeasurement(sorBytes);
            await TryCloseAsync(true);
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            IsOpen = false;
            return true;
        }

        public async void Cancel()
        {
            Message = Resources.SID_Interrupting_Measurement__Client___Wait_please___;
            IsCancelButtonEnabled = false;
            if (_rtu.RtuMaker == RtuMaker.VeEX)
                _cts.Cancel();
            await _measurementInterrupter.Interrupt(_rtu, @"measurement (Client)");
            await TryCloseAsync();
        }
    }
}
