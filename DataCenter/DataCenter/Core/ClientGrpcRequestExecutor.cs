using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter;

public class ClientGrpcRequestExecutor
{
    private readonly ClientCollection _clientCollection;
    private readonly RtuOccupations _rtuOccupations;

    public ClientGrpcRequestExecutor(ClientCollection clientCollection, RtuOccupations rtuOccupations)
    {
        _clientCollection = clientCollection;
        _rtuOccupations = rtuOccupations;
    }

    public async Task<RequestAnswer> ExecuteRequest(object o)
    {
        switch (o)
        {
            case CheckServerConnectionDto dto:
                return new RequestAnswer(ReturnCode.Ok);
            case RegisterClientDto dto:
                return await _clientCollection.RegisterClientAsync(dto);
            case RegisterHeartbeatDto dto:
                return await _clientCollection.RegisterHeartbeat(dto);
            case SetRtuOccupationDto dto:
                return await _rtuOccupations.SetRtuOccupationState(dto);
            default: return new RequestAnswer(ReturnCode.Error);
        }
    }

}