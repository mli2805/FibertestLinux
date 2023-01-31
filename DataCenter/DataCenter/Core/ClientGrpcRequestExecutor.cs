using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter;

public class ClientGrpcRequestExecutor
{
    private readonly ClientCollection _clientCollection;
    private readonly RtuOccupations _rtuOccupations;
    private readonly EventStoreService _eventStoreService;
    private readonly DiskSpaceProvider _diskSpaceProvider;

    public ClientGrpcRequestExecutor(ClientCollection clientCollection, RtuOccupations rtuOccupations,
        EventStoreService eventStoreService, DiskSpaceProvider diskSpaceProvider)
    {
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
            case RegisterHeartbeatDto dto:
                return await _clientCollection.RegisterHeartbeat(dto);
            case SetRtuOccupationDto dto:
                return await _rtuOccupations.SetRtuOccupationState(dto);

            case GetEventsDto dto:
                return _eventStoreService.GetEvents(dto.Revision);
            case GetDiskSpaceDto dto:
                return await _diskSpaceProvider.GetDiskSpaceGb();

            default: return new RequestAnswer(ReturnCode.Error);
        }
    }

}