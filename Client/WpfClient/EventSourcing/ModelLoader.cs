using System;
using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class ModelLoader
    {
        private readonly ILogger _logger; 
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly GraphReadModel _graphReadModel;
        private readonly ZoneEventsOnTreeExecutor _zoneEventsOnTreeExecutor;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;

        public ModelLoader(ILogger logger, Model readModel, IWcfServiceDesktopC2D c2DWcfManager, GraphReadModel graphReadModel,
            ZoneEventsOnTreeExecutor zoneEventsOnTreeExecutor,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel)
        {
            _logger = logger;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _graphReadModel = graphReadModel;
            _zoneEventsOnTreeExecutor = zoneEventsOnTreeExecutor;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
        }

        public async Task<int> DownloadAndApplyModel()
        {
            try
            {
                _logger.LogInfo(Logs.Client,@"Downloading model...");
                var dto = await _c2DWcfManager.GetModelDownloadParams(new GetSnapshotDto());
                _logger.LogInfo(Logs.Client,
                    $@"Model size is {dto.Size} in {dto.PortionsCount} portions, last event included {dto.LastIncludedEvent}");

                var bb = new byte[dto.Size];
                var offset = 0;

                for (int i = 0; i < dto.PortionsCount; i++)
                {
                    var portion = await _c2DWcfManager.GetModelPortion(i);
                    portion.CopyTo(bb, offset);
                    offset += portion.Length;
                    _logger.LogInfo(Logs.Client,$@"portion {i}  {portion.Length} bytes received");
                }

                await _readModel.Deserialize(_logger, bb);
                await _graphReadModel.RefreshVisiblePart();

                _zoneEventsOnTreeExecutor.RenderOfModelAfterSnapshot();
                _opticalEventsDoubleViewModel.RenderMeasurementsFromSnapshot();
                _networkEventsDoubleViewModel.RenderNetworkEvents();
                _bopNetworkEventsDoubleViewModel.RenderBopNetworkEvents();

                return dto.LastIncludedEvent;
            }
            catch (Exception e)
            {
                _logger.LogError(Logs.Client,$@"DownloadModel : {e.Message}");
                return -1;
            }
        }

    }
}
