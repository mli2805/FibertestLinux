using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using GrpsClientLib;
using Microsoft.Extensions.Logging;
using NEventStore;
using Newtonsoft.Json;

namespace Fibertest.WpfClient
{
    public class ClientPoller : PropertyChangedBase
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly IWindowManager _windowManager;
        private readonly Model _readModel;
        private readonly ServerConnectionLostViewModel _serverConnectionLostViewModel;
        private readonly IWcfServiceInSuperClient _c2SWcfManager;
        private readonly SystemState _systemState;
        private readonly CurrentUser _currentUser;
        private readonly CommandLineParameters _commandLineParameters;
        private readonly EventsOnGraphExecutor _eventsOnGraphExecutor;
        private readonly DataCenterConfig _currentDatacenterParameters;
        private readonly EventLogComposer _eventLogComposer;
        private readonly EventsOnTreeExecutor _eventsOnTreeExecutor;
        private readonly OpticalEventsExecutor _opticalEventsExecutor;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly RtuChannelViewsManager _rtuChannelViewsManager;
        private readonly BopStateViewsManager _bopStateViewsManager;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly LandmarksViewsManager _landmarksViewsManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private int _exceptionCount;
        private readonly int _exceptionCountLimit;
        private readonly ILogger _logger; 
        private readonly EventArrivalNotifier _eventArrivalNotifier;
        private readonly int _pollingRate;

        private int _currentEventNumber;
        public int CurrentEventNumber
        {
            get => _currentEventNumber;
            set
            {
                _currentEventNumber = value;
                // some forms refresh their view because they have sent command previously and are waiting event's arrival
                _eventArrivalNotifier.NeverMind = _currentEventNumber;
            }
        }

        public ClientPoller(GrpcC2DRequests grpcC2DRequests, IDispatcherProvider dispatcherProvider,
            IWindowManager windowManager, Model readModel,
            ServerConnectionLostViewModel serverConnectionLostViewModel,
            IWcfServiceInSuperClient c2SWcfManager, SystemState systemState, CurrentUser currentUser,
            CommandLineParameters commandLineParameters, DataCenterConfig currentDatacenterParameters,

            EventLogComposer eventLogComposer, EventsOnGraphExecutor eventsOnGraphExecutor,
            EventsOnTreeExecutor eventsOnTreeExecutor, OpticalEventsExecutor opticalEventsExecutor,

            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
            RtuStateViewsManager rtuStateViewsManager, RtuChannelViewsManager rtuChannelViewsManager,
            BopStateViewsManager bopStateViewsManager, NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel, LandmarksViewsManager landmarksViewsManager,

            ILogger logger, IWritableConfig<ClientConfig> config, EventArrivalNotifier eventArrivalNotifier)
        {
            _grpcC2DRequests = grpcC2DRequests;
            _windowManager = windowManager;
            _readModel = readModel;
            _serverConnectionLostViewModel = serverConnectionLostViewModel;
            _c2SWcfManager = c2SWcfManager;
            _systemState = systemState;
            _currentUser = currentUser;
            _commandLineParameters = commandLineParameters;
            _eventsOnGraphExecutor = eventsOnGraphExecutor;
            _currentDatacenterParameters = currentDatacenterParameters;
            _eventLogComposer = eventLogComposer;
            _eventsOnTreeExecutor = eventsOnTreeExecutor;
            _opticalEventsExecutor = opticalEventsExecutor;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _rtuStateViewsManager = rtuStateViewsManager;
            _rtuChannelViewsManager = rtuChannelViewsManager;
            _bopStateViewsManager = bopStateViewsManager;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _landmarksViewsManager = landmarksViewsManager;
            _dispatcherProvider = dispatcherProvider;
            _logger = logger;
            _eventArrivalNotifier = eventArrivalNotifier;
            _pollingRate = config.Value.General.ClientPollingRateMs;
            _exceptionCountLimit = config.Value.General.FailedPollsLimit;
        }

        public void Start(CancellationToken token)
        {
            _logger.LogInfo(Logs.Client,$@"Polling started from {_currentEventNumber + 1}");
            _eventLogComposer.Initialize();
            var pollerThread = new Thread(() => DoPolling(token)) { IsBackground = true };
            pollerThread.Start();
        }

        private async void DoPolling(CancellationToken token)
        {
            if (_commandLineParameters.IsUnderSuperClientStart)
                _systemState.PropertyChanged += _systemState_PropertyChanged;
            while (!token.IsCancellationRequested)
            {
                await EventSourcingTick();
                Thread.Sleep(TimeSpan.FromMilliseconds(_pollingRate));
            }
            _logger.LogInfo(Logs.Client,@"Leaving DoPolling...");
        }

        private void _systemState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _logger.LogInfo(Logs.Client,@"Notify super-client system state changed.");
            _c2SWcfManager.SetSystemState(_commandLineParameters.ClientOrdinal, !_systemState.HasAnyActualProblem);
        }

        public async Task<int> EventSourcingTick()
        {
            var getEventsDto = new GetEventsDto() { Revision = CurrentEventNumber, ClientConnectionId = _currentUser.ConnectionId };
            var result = await _grpcC2DRequests.SendAnyC2DRequest<GetEventsDto, EventsDto>(getEventsDto);
            string[]? events = result.Events;

            if (events == null)
            {
                _exceptionCount++;
                _logger.LogError(Logs.Client,$@"Cannot establish connection with data-center. Exception count: {_exceptionCount}");
                if (_exceptionCount == _exceptionCountLimit) // blocks current thread till user clicks to close form
                    _dispatcherProvider.GetDispatcher().Invoke(NotifyUserConnectionProblems);
                return -1;
            }

            _exceptionCount = 0;

            if (events.Length == 0)
                return 0;

            _dispatcherProvider.GetDispatcher().Invoke(() => ApplyEventSourcingEvents(events)); // sync, GUI thread
            return events.Length;
        }

        private async void NotifyUserConnectionProblems()
        {
            _serverConnectionLostViewModel.Initialize(_currentDatacenterParameters.General.ServerTitle,
                _currentDatacenterParameters.General.ServerDoubleAddress.Main.Ip4Address);
            _serverConnectionLostViewModel.PropertyChanged += OnServerConnectionLostViewModelOnPropertyChanged;
            await _windowManager.ShowDialogWithAssignedOwner(_serverConnectionLostViewModel);
        }

        private void OnServerConnectionLostViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"IsOpen")
            {
                if (_commandLineParameters.IsUnderSuperClientStart)
                    _c2SWcfManager.NotifyConnectionBroken(_commandLineParameters.ClientOrdinal);
                Application.Current.Shutdown();
            }
        }

        private async void ApplyEventSourcingEvents(string[] events)
        {
            foreach (var json in events)
            {
                var o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                if (o == null) continue;
                var msg = (EventMessage)o;
                var evnt = msg.Body;

                try
                {
                    _readModel.Apply(evnt);

                    await _eventsOnGraphExecutor.Apply(evnt);
                    _eventsOnTreeExecutor.Apply(evnt);
                    _opticalEventsExecutor.Apply(evnt);
                    _networkEventsDoubleViewModel.Apply(evnt);
                    _rtuStateViewsManager.Apply(evnt);
                    _traceStateViewsManager.Apply(evnt);
                    _traceStatisticsViewsManager.Apply(evnt);
                    _landmarksViewsManager.Apply(evnt);
                    _rtuChannelViewsManager.Apply(evnt);
                    _bopStateViewsManager.Apply(evnt);
                    _bopNetworkEventsDoubleViewModel.Apply(evnt);

                    _eventLogComposer.AddEventToLog(msg);
                }
                catch (Exception e)
                {
                    _logger.LogError(Logs.Client,e.Message);
                    var header = @"Timestamp";
                    _logger.LogError(Logs.Client,$@"Exception thrown while processing event with timestamp {msg.Headers[header]} \n {evnt.GetType().FullName}");
                }

                CurrentEventNumber++;
            }
        }
    }
}