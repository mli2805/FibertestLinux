using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fibertest.WpfClient
{
    public class GrpcInClientProcessor
    {
        private readonly ILogger _logger;
        private readonly IWindowManager _windowManager;
        private readonly Heartbeater _heartbeater;
        private readonly ClientPoller _clientPoller;
        private readonly CurrentUser _currentUser;
        private readonly WaitViewModel _waitViewModel;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;
        private readonly AutoBaseViewModel _autoBaseViewModel;
        private readonly RtuAutoBaseViewModel _rtuAutoBaseViewModel;
        private readonly RtuStateViewsManager _rtuStateViewsManager;

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        public GrpcInClientProcessor(ILogger logger, IWindowManager windowManager,
            Heartbeater heartbeater, ClientPoller clientPoller, CurrentUser currentUser,
            WaitViewModel waitViewModel, ClientMeasurementViewModel clientMeasurementViewModel,
            AutoBaseViewModel autoBaseViewModel, RtuAutoBaseViewModel rtuAutoBaseViewModel,
            RtuStateViewsManager rtuStateViewsManager)
        {
            _logger = logger;
            _windowManager = windowManager;
            _heartbeater = heartbeater;
            _clientPoller = clientPoller;
            _currentUser = currentUser;
            _waitViewModel = waitViewModel;
            _clientMeasurementViewModel = clientMeasurementViewModel;
            _autoBaseViewModel = autoBaseViewModel;
            _rtuAutoBaseViewModel = rtuAutoBaseViewModel;
            _rtuStateViewsManager = rtuStateViewsManager;
        }

        public async Task Apply(string json)
        {
            var o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
            switch (o)
            {
                case ClientMeasurementResultDto dto:
                    ProcessMeasurementResult(dto);
                    break;
                case CurrentMonitoringStepDto dto:
                    ProcessCurrentMonitoringStep(dto);
                    break;
                case DbOptimizationProgressDto dto:
                    await ProcessDbOptimizationProgress(dto);
                    break;
                case ServerAsksClientToExitDto dto:
                    await ProcessServerAsksClientToExit(dto);
                    break;

                default: return;
            }
        }

        private async Task ProcessDbOptimizationProgress(DbOptimizationProgressDto dto)
        {
            _logger.Info(Logs.Client, $@"DbOptimizationProgressDto received, {dto.Stage}");

            if (!_waitViewModel.IsOpen)
                await Task.Factory.StartNew(ShowWaiting);
            else
                _waitViewModel.UpdateOptimizationProgress(dto);
        }

        private async Task ProcessServerAsksClientToExit(ServerAsksClientToExitDto dto)
        {
            _logger.Info(Logs.Client, $"Server asks client to exit. {dto.Reason}");
            await Task.Factory.StartNew(() => LeaveApp(dto.Reason));
        }

        private void ProcessMeasurementResult(ClientMeasurementResultDto dto)
        {
            _logger.Info(Logs.Client, $"Client measurement result {dto.ReturnCode.GetLocalizedString()}");
            if (_clientMeasurementViewModel.IsOpen)
                Task.Factory.StartNew(() => _clientMeasurementViewModel.ShowResult(dto));
            if (_autoBaseViewModel.IsOpen)
                Task.Factory.StartNew(() => _autoBaseViewModel.OneMeasurementExecutor.ProcessMeasurementResult(dto));
            if (_rtuAutoBaseViewModel.IsOpen)
                Task.Factory.StartNew(() => _rtuAutoBaseViewModel.WholeRtuMeasurementsExecutor.ProcessMeasurementResult(dto));
        }

        private void ProcessCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            _logger.Info(Logs.Client, $"Current monitoring step {dto.Step}");
            _rtuStateViewsManager.NotifyUserRtuCurrentMonitoringStep(dto);
        }

        private void ShowWaiting()
        {
            _heartbeater.CancellationTokenSource.Cancel();
            _clientPoller.CancellationTokenSource.Cancel();
            _waitViewModel.Initialize(LongOperation.DbOptimization);
            Application.Current.Dispatcher?.InvokeAsync(() => _windowManager.ShowDialogWithAssignedOwner(_waitViewModel));
        }

        private async Task<int> LeaveApp(UnRegisterReason reason)
        {
            _heartbeater.CancellationTokenSource.Cancel();
            _clientPoller.CancellationTokenSource.Cancel();
            var vm = new LeaveAppViewModel();
            vm.Initialize(reason, _currentUser.UserName);
            if (Application.Current.Dispatcher != null)
                await Application.Current.Dispatcher.InvokeAsync(() => _windowManager.ShowDialogWithAssignedOwner(vm));
            return 0;
        }
    }
}
