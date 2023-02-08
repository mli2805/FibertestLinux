using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class TraceLeafActions
    {
        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2DCommonWcf;
        private readonly TabulatorViewModel _tabulatorViewModel;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly BaseRefsAssignViewModel _baseRefsAssignViewModel;
        private readonly LandmarksViewsManager _landmarksViewsManager;
        private readonly OutOfTurnPreciseMeasurementViewModel _outOfTurnPreciseMeasurementViewModel;
        private readonly AutoBaseViewModel _autoBaseViewModel;
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;

        public TraceLeafActions(ILifetimeScope globalScope, CurrentUser currentUser, 
            Model readModel, GraphReadModel graphReadModel,
            IWindowManager windowManager, TabulatorViewModel tabulatorViewModel,
            GrpcC2DService grpcC2DService, IWcfServiceCommonC2D c2DCommonWcf,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
            BaseRefsAssignViewModel baseRefsAssignViewModel, LandmarksViewsManager landmarksViewsManager,
            OutOfTurnPreciseMeasurementViewModel outOfTurnPreciseMeasurementViewModel, AutoBaseViewModel autoBaseViewModel,
            CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _windowManager = windowManager;
            _c2DCommonWcf = c2DCommonWcf;
            _tabulatorViewModel = tabulatorViewModel;
            _grpcC2DService = grpcC2DService;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _baseRefsAssignViewModel = baseRefsAssignViewModel;
            _landmarksViewsManager = landmarksViewsManager;
            _outOfTurnPreciseMeasurementViewModel = outOfTurnPreciseMeasurementViewModel;
            _autoBaseViewModel = autoBaseViewModel;
            _commonStatusBarViewModel = commonStatusBarViewModel;
        }

        public async Task UpdateTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            if (!_readModel.TryGetTrace(traceLeaf.Id, out Trace? trace))
                return;

            var vm = _globalScope.Resolve<TraceInfoViewModel>();
            await vm.Initialize(traceLeaf.Id, trace!.EquipmentIds, trace.NodeIds, false);
            await _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public async Task ShowTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;
            if (_tabulatorViewModel.SelectedTabIndex != 3)
                _tabulatorViewModel.SelectedTabIndex = 3;
            await Task.Delay(100);

            var trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            if (_graphReadModel.ForcedTraces.All(t => t.TraceId != traceLeaf.Id))
                _graphReadModel.ForcedTraces.Add(trace);

            _graphReadModel.ShowTrace(trace);
        }

        public async Task AssignBaseRefs(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            await Task.Delay(0);
            var trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            _baseRefsAssignViewModel.Initialize(trace);
            await _windowManager.ShowDialogWithAssignedOwner(_baseRefsAssignViewModel);
        }

        public async Task ShowTraceState(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            await Task.Delay(0);
            _traceStateViewsManager.ShowTraceState(traceLeaf.Id);
        }

        public async Task ShowTraceStatistics(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            await Task.Delay(0);
            _traceStatisticsViewsManager.Show(traceLeaf.Id);
        }

        public async Task ShowTraceLandmarks(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;
            var rtuNodeId = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id).NodeIds[0];
            await _landmarksViewsManager.InitializeFromTrace(traceLeaf.Id, rtuNodeId);
        }

        public async Task DetachTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            var detachTraceDto = new DetachTraceDto
            {
                TraceId = traceLeaf.Id,
                ClientConnectionId = _currentUser.ConnectionId,
            };

            var result = await _c2DCommonWcf.DetachTraceAsync(detachTraceDto);

            if (result.ReturnCode != ReturnCode.Ok)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error,
                    new List<string>() { result.ReturnCode.GetLocalizedString() });
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        public async Task CleanTrace(object param)
        {
            await DoCleanOrRemoveTrace(param, false);
        }

        public async Task RemoveTrace(object param)
        {
            await DoCleanOrRemoveTrace(param, true);
        }

        private async Task DoCleanOrRemoveTrace(object param, bool isRemoval)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;
            var traceId = traceLeaf.Id;
            var rtuId = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id).RtuId;
            var rtu = _readModel.Rtus.First(r => r.Id == rtuId);
            if (!await _globalScope.Resolve<IRtuHolder>().SetRtuOccupationState(rtu.Id, rtu.Title, RtuOccupation.CleanOrRemoveTrace))
                return;

            var question = AssembleTraceRemovalConfirmation(traceLeaf.Title);
            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, question);
            await _windowManager.ShowDialogWithAssignedOwner(vm);

            if (vm.IsAnswerPositive)
            {
                _commonStatusBarViewModel.StatusBarMessage2 = Resources.SID_Long_operation__Removing_trace_s_measurements____Please_wait_;
                var cmd = isRemoval ? new RemoveTrace() { TraceId = traceId } : (object)new CleanTrace() { TraceId = traceId };
                var result = await _grpcC2DService.SendEventSourcingCommand(cmd);
                _commonStatusBarViewModel.StatusBarMessage2 = result.ReturnCode == ReturnCode.Ok ? "" : result.ErrorMessage!;
            }

            await _globalScope.Resolve<IRtuHolder>()
                .SetRtuOccupationState(rtu.Id, rtu.Title, RtuOccupation.None);
        }

        private static List<MyMessageBoxLineModel> AssembleTraceRemovalConfirmation(string traceTitle)
        {
            var list = new List<MyMessageBoxLineModel>
            {
                new MyMessageBoxLineModel() {Line = Resources.SID_Attention_},
                new MyMessageBoxLineModel() {Line = Resources.SID_All_measurements_for_trace},
                new MyMessageBoxLineModel() {Line = ""},
                new MyMessageBoxLineModel() {Line = $@"{traceTitle}", FontWeight = FontWeights.Bold},
                new MyMessageBoxLineModel() {Line = ""},
                new MyMessageBoxLineModel() {Line = Resources.SID_will_be_removed},
                new MyMessageBoxLineModel() {Line = ""},
                new MyMessageBoxLineModel() {Line = Resources.SID_Are_you_sure_},
            };
            return list;
        }

        public async Task DoPreciseMeasurementOutOfTurn(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            var rtuId = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id).RtuId;
            var rtu = _readModel.Rtus.First(r => r.Id == rtuId);

            _outOfTurnPreciseMeasurementViewModel.Initialize(traceLeaf);
            await _windowManager.ShowDialogWithAssignedOwner(_outOfTurnPreciseMeasurementViewModel);

            await _globalScope.Resolve<IRtuHolder>()
                .SetRtuOccupationState(rtuId, rtu.Title, RtuOccupation.None);
        }

        public async Task AssignBaseRefsAutomatically(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            await Task.Delay(0);

            var rtuId = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id).RtuId;
            var rtu = _readModel.Rtus.First(r => r.Id == rtuId);
            if (!await _globalScope.Resolve<IRtuHolder>().SetRtuOccupationState(rtuId, rtu.Title, RtuOccupation.DoAutoBaseMeasurement))
                return;

            if (!_autoBaseViewModel.Initialize(traceLeaf))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error,
                    @"Can't start auto base assignment without RFTS template file!");
                await _windowManager.ShowDialogWithAssignedOwner(mb);
                return;
            }
            await _windowManager.ShowDialogWithAssignedOwner(_autoBaseViewModel);
        }
    }
}