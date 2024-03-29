﻿using System;
using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class ModelLoader
    {
        private readonly ILogger _logger; 
        private readonly Model _readModel;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly GraphReadModel _graphReadModel;
        private readonly ZoneEventsOnTreeExecutor _zoneEventsOnTreeExecutor;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;

        public ModelLoader(ILogger logger, Model readModel, GrpcC2DService grpcC2DService, GraphReadModel graphReadModel,
            ZoneEventsOnTreeExecutor zoneEventsOnTreeExecutor,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel)
        {
            _logger = logger;
            _readModel = readModel;
            _grpcC2DService = grpcC2DService;
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
                _logger.Info(Logs.Client,@"Downloading model...");
                var paramsDto =
                    await _grpcC2DService
                        .SendAnyC2DRequest<GetSerializedModelParamsDto, SerializedModelDto>(new GetSerializedModelParamsDto());
                _logger.Info(Logs.Client,
                    $@"Model size is {paramsDto.Size} in {paramsDto.PortionsCount} portions, last event included {paramsDto.LastIncludedEvent}");
                
                // var bb = new byte[paramsDto.Size];
                // var offset = 0;
                //
                // for (int i = 0; i < paramsDto.PortionsCount; i++)
                // {
                //
                //     var result =
                //         await _grpcC2DService.SendAnyC2DRequest<GetModelPortionDto, SerializedModelPortionDto>(
                //             new GetModelPortionDto(i));
                //     result.Bytes.CopyTo(bb, offset);
                //     offset += result.Bytes.Length;
                //     _logger.Info(Logs.Client,$@"portion {i}  {result.Bytes.Length} bytes received");
                // }

                var bb2 = await _grpcC2DService.DownloadModel(paramsDto.Size);

                await _readModel.Deserialize(_logger, bb2);
                await _graphReadModel.RefreshVisiblePart();

                _zoneEventsOnTreeExecutor.RenderOfModelAfterSnapshot();
                _opticalEventsDoubleViewModel.RenderMeasurementsFromSnapshot();
                _networkEventsDoubleViewModel.RenderNetworkEvents();
                _bopNetworkEventsDoubleViewModel.RenderBopNetworkEvents();

                return paramsDto.LastIncludedEvent;
            }
            catch (Exception e)
            {
                _logger.Error(Logs.Client,$@"DownloadModel : {e.Message}");
                return -1;
            }
        }

    }
}
