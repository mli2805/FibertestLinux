using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using GMap.NET;
using Microsoft.Extensions.Logging;

// ReSharper disable ForCanBeConvertedToForeach

namespace Fibertest.WpfClient
{
    public class GraphReadModel : PropertyChangedBase
    {
        public Map MainMap { get; set; }

        private bool _isInGisVisibleMode = true;
        public bool IsInGisVisibleMode
        {
            get { return _isInGisVisibleMode; }
            set
            {
                if (value == _isInGisVisibleMode) return;
                _isInGisVisibleMode = value;
                NotifyOfPropertyChange();
            }
        }

        public CurrentGis CurrentGis { get; }
        public CurrentUser CurrentUser { get; }
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; }
        public GrmNodeRequests GrmNodeRequests { get; }
        public GrmEquipmentRequests GrmEquipmentRequests { get; }
        public GrmFiberRequests GrmFiberRequests { get; }
        public GrmFiberWithNodesRequest GrmFiberWithNodesRequest { get; }
        public GrmRtuRequests GrmRtuRequests { get; }
        public IWindowManager WindowManager { get; }
        public Model ReadModel { get; }
        public readonly ILifetimeScope GlobalScope;
        public readonly IWritableConfig<ClientConfig> Config;
        public ILogger<GraphReadModel> Logger { get; private set; }

        public GraphReadModelData Data { get; set; } = new GraphReadModelData();
        public List<Trace> ForcedTraces { get; set; } = new List<Trace>();

        public List<GraphVisibilityLevelItem> GraphVisibilityItems { get; set; }
        private GraphVisibilityLevelItem _selectedGraphVisibilityItem;

        public GraphVisibilityLevelItem SelectedGraphVisibilityItem
        {
            get => _selectedGraphVisibilityItem;
            set
            {
                if (value == _selectedGraphVisibilityItem) return;
                _selectedGraphVisibilityItem = value;
                Config.Update(c=>c.Miscellaneous.GraphVisibilityLevel = SelectedGraphVisibilityItem.Level);
                NotifyOfPropertyChange();
            }
        }

        public GraphReadModel(ILifetimeScope globalScope, IWritableConfig<ClientConfig> config, ILogger<GraphReadModel> logger,
            CurrentGis currentGis, CurrentUser currentUser,
            CommonStatusBarViewModel commonStatusBarViewModel,
            GrmNodeRequests grmNodeRequests, GrmEquipmentRequests grmEquipmentRequests,
            GrmFiberRequests grmFiberRequests, GrmFiberWithNodesRequest grmFiberWithNodesRequest,
             GrmRtuRequests grmRtuRequests,
            IWindowManager windowManager, Model readModel)
        {
            CurrentGis = currentGis;
            currentGis.PropertyChanged += CurrentGis_PropertyChanged;
            CurrentUser = currentUser;
            CommonStatusBarViewModel = commonStatusBarViewModel;
            GrmNodeRequests = grmNodeRequests;
            GrmEquipmentRequests = grmEquipmentRequests;
            GrmFiberRequests = grmFiberRequests;
            GrmFiberWithNodesRequest = grmFiberWithNodesRequest;
            GrmRtuRequests = grmRtuRequests;
            WindowManager = windowManager;
            ReadModel = readModel;
            GlobalScope = globalScope;
            Config = config;
            Logger = logger;
            Data.Nodes = new ObservableCollection<NodeVm>();
            Data.Fibers = new ObservableCollection<FiberVm>();

            GraphVisibilityItems = GraphVisibilityExt.GetComboboxItems();
            var level = Config.Value.Miscellaneous.GraphVisibilityLevel;
           
            SetGraphVisibility(level);
        }

        private void CurrentGis_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsInGisVisibleMode = ((CurrentGis)sender).IsGisOn;
        }

        public async Task<int> RefreshVisiblePart()
        {
            var renderingResult = CurrentUser.ZoneId == Guid.Empty
                ? await this.RenderForFullGraphUser()
                : await this.RenderForZoneUser(CurrentUser.ZoneId);

            var nodeVmCount = await this.ToExistingGraph(renderingResult);

            if (MainMap != null)
                MainMap.NodeCountString = $@" {ReadModel.Nodes.Count} / {nodeVmCount}";

            return nodeVmCount;
        }

        public void SetGraphVisibility(GraphVisibilityLevel level)
        {
            SelectedGraphVisibilityItem =
                GraphVisibilityItems.First(i => i.Level == level);
                Config.Update(c=>c.Miscellaneous.GraphVisibilityLevel = level);
        }

        public void PlacePointIntoScreenCenter(PointLatLng position)
        {
            MainMap.SetPosition(position);
        }

        public void ShowTrace(Trace trace)
        {
            // RTU into screen center
            var node = ReadModel.Nodes.First(n => n.NodeId == trace.NodeIds[0]);
            MainMap.SetPosition(node.Position);

            HighlightTrace(trace);
        }

        public void NodeToCenterAndHighlight(Guid nodeId)
        {
            var node = ReadModel.Nodes.First(n => n.NodeId == nodeId);
            MainMap.SetPosition(node.Position);

            node.IsHighlighted = true;
            var nodeVm = Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (nodeVm != null)
                nodeVm.IsHighlighted = true;
        }

        public void HighlightTrace(Trace trace)
        {
            SetTraceLight(trace, true);
        }

        public void ExtinguishTrace(Trace trace)
        {
            SetTraceLight(trace, false);
        }

        private void SetTraceLight(Trace trace, bool highlight)
        {
            foreach (var fiberId in trace.FiberIds)
            {
                ReadModel.Fibers.First(f => f.FiberId == fiberId).SetLightOnOff(trace.TraceId, highlight);

                var fiberVm = Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                if (fiberVm != null)
                    fiberVm.SetLightOnOff(trace.TraceId, highlight);
            }
        }

        public void ExtinguishAll()
        {
            ForcedTraces.Clear();
            ExtinguishAllNodes();
            ExtinguishAllFibers();
        }

        public void ExtinguishAllNodes()
        {
            for (int i = 0; i < Data.Nodes.Count; i++)
                Data.Nodes[i].IsHighlighted = false;
            for (int i = 0; i < ReadModel.Nodes.Count; i++)
                ReadModel.Nodes[i].IsHighlighted = false;
        }

        private void ExtinguishAllFibers()
        {
            for (int i = 0; i < Data.Fibers.Count; i++)
                Data.Fibers[i].ClearLight();
            for (int i = 0; i < ReadModel.Fibers.Count; i++)
                ReadModel.Fibers[i].HighLights = new List<Guid>();
        }

        public void ChangeTraceColor(Guid traceId, FiberState state)
        {
            var trace = ReadModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null) return;

            var fibers = ReadModel.GetTraceFibers(trace);
            foreach (var fiber in fibers)
            {
                var fiberVm = Data.Fibers.FirstOrDefault(f => f.Id == fiber.FiberId);
                if (fiberVm == null) continue;
                if (state != FiberState.NotInTrace)
                    fiberVm.SetState(traceId, state);
                else
                    fiberVm.RemoveState(traceId);
            }
        }

        public void SetFutureTraceLightOnOff(Guid traceId, List<Guid> fiberIds, bool light)
        {
            foreach (var fiberId in fiberIds)
            {
                var fiber = ReadModel.Fibers.First(f => f.FiberId == fiberId);
                fiber.SetLightOnOff(traceId, light);

                var fiberVm = Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                fiberVm?.SetLightOnOff(traceId, light);
            }
        }

        public void ChangeFutureTraceColor(Guid traceId, List<Guid> fiberIds, FiberState state)
        {
            foreach (var fiberId in fiberIds)
            {
                var fiberVm = Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                if (fiberVm != null)
                {
                    if (state != FiberState.NotInTrace)
                        fiberVm.SetState(traceId, state);
                    else
                        fiberVm.RemoveState(traceId);
                }
            }
        }
    }
}