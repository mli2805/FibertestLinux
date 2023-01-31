using System;
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
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class AutoBaseViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly ILogger _logger; 
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly ReflectogramManager _reflectogramManager;
        private TraceLeaf _traceLeaf;
        private Rtu _rtu;

        public bool IsOpen { get; set; }
        public IOneMeasurementExecutor OneMeasurementExecutor { get; set; }
        public bool IsShowRef { get; set; }

        public AutoBaseViewModel(ILifetimeScope globalScope, ILogger logger, Model readModel, 
            IWindowManager windowManager, IDispatcherProvider dispatcherProvider,
            ReflectogramManager reflectogramManager)
        {
            _globalScope = globalScope;
            _logger = logger;
            _readModel = readModel;
            _windowManager = windowManager;
            _dispatcherProvider = dispatcherProvider;
            _reflectogramManager = reflectogramManager;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
            IsOpen = true;
            OneMeasurementExecutor.MeasurementCompleted += OneMeasurementExecutor_MeasurementCompleted;
        }

        public bool Initialize(TraceLeaf traceLeaf)
        {
            _traceLeaf = traceLeaf;

            var trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            _rtu = _readModel.Rtus.First(r => r.Id == trace.RtuId);

            OneMeasurementExecutor = _rtu.RtuMaker == RtuMaker.IIT 
                ? _globalScope.Resolve<OneIitMeasurementExecutor>()
                : _globalScope.Resolve<OneVeexMeasurementExecutor>();
            if (!OneMeasurementExecutor.Initialize(_rtu, false))
                return false;

            IsShowRef = true;
            OneMeasurementExecutor.Model.IsEnabled = true;

            return true;
        }

        private void OneMeasurementExecutor_MeasurementCompleted(object sender, EventArgs e)
        {
            var result = (MeasurementEventArgs)e;
            _logger.LogInfo(Logs.Client,$@"Measurement on trace {_traceLeaf.Title}: {result.Code}");

            _dispatcherProvider.GetDispatcher().Invoke(() => Finish(result));
        }

        private async void Finish(MeasurementEventArgs result)
        {
            _waitCursor.Dispose();
            OneMeasurementExecutor.Model.MeasurementProgressViewModel.DisplayStop();
            OneMeasurementExecutor.Model.IsEnabled = true;

            if (result.Code == ReturnCode.BaseRefAssignedSuccessfully)
            {
                if (IsShowRef)
                    _reflectogramManager.ShowClientMeasurement(result.SorBytes);
                await TryCloseAsync();
            }
            else
            {
                var strings = new List<string>() { result.Code.GetLocalizedString() };
                if (result.AdditionalErrorLines[0] != "")
                    strings.AddRange(result.AdditionalErrorLines);
                var vm = new MyMessageBoxViewModel(MessageType.Error, strings, 0);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        private WaitCursor _waitCursor;
        public async void Start()
        {
            _waitCursor = new WaitCursor();
            OneMeasurementExecutor.Model.IsEnabled = false;
            await OneMeasurementExecutor.Start(_traceLeaf);
        }


        public async void Close()
        {
            await TryCloseAsync();
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            IsOpen = false;
            OneMeasurementExecutor.MeasurementCompleted -= OneMeasurementExecutor_MeasurementCompleted;

            await _globalScope.Resolve<IRtuHolder>().SetRtuOccupationState(_rtu.Id, _rtu.Title, RtuOccupation.None);
            return true;
        }
    }
}
