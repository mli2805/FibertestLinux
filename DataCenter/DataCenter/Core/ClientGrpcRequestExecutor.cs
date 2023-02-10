using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public partial class ClientGrpcRequestExecutor
{
    private readonly IWritableConfig<DataCenterConfig> _config;
    private readonly ILogger<ClientGrpcRequestExecutor> _logger;
    private readonly Model _writeModel;
    private readonly ClientCollection _clientCollection;
    private readonly RtuOccupations _rtuOccupations;
    private readonly EventStoreService _eventStoreService;
    private readonly DiskSpaceProvider _diskSpaceProvider;

    public ClientGrpcRequestExecutor(IWritableConfig<DataCenterConfig> config, 
        ILogger<ClientGrpcRequestExecutor> logger, Model writeModel,
        ClientCollection clientCollection, RtuOccupations rtuOccupations,
        EventStoreService eventStoreService, DiskSpaceProvider diskSpaceProvider)
    {
        _config = config;
        _logger = logger;
        _writeModel = writeModel;
        _clientCollection = clientCollection;
        _rtuOccupations = rtuOccupations;
        _eventStoreService = eventStoreService;
        _diskSpaceProvider = diskSpaceProvider;
    }

    public async Task<RequestAnswer> ExecuteRequest(object o)
    {
        switch (o)
        {
            case CheckServerConnectionDto _:
                return new RequestAnswer(ReturnCode.Ok);
            case RegisterClientDto dto:
                return await _clientCollection.RegisterClientAsync(dto);
            case UnRegisterClientDto dto:
                return await _clientCollection.UnRegisterClientAsync(dto);
            case ClientHeartbeatDto dto:
                return await _clientCollection.RegisterHeartbeat(dto);
            case SetRtuOccupationDto dto:
                return await _rtuOccupations.SetRtuOccupationState(dto);

            case GetDiskSpaceDto _:
                return await _diskSpaceProvider.GetDiskSpaceGb();
            case GetEventsDto dto:
                return _eventStoreService.GetEvents(dto.Revision);
            case GetSerializedModelParamsDto _:
                return await GetModelDownloadParams();
            // case GetModelPortionDto dto:
            //     return await GetModelPortion(dto.Portion);

            case ChangeDcConfigDto dto:
                _config.Update(cfg=>cfg.FillIn(dto.NewConfig));
                return new RequestAnswer(ReturnCode.Ok);
         
            default: return new RequestAnswer(ReturnCode.Error);
        }
    }

}