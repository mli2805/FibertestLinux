using Fibertest.Graph;

namespace Fibertest.DataCenter;

public partial class RtuResponseToEventSourcing
{
    private readonly ILogger<RtuResponseToEventSourcing> _logger;
    private readonly Model _writeModel;
    private readonly EventStoreService _eventStoreService;
    private readonly ClientCollection _clientCollection;

    public RtuResponseToEventSourcing(ILogger<RtuResponseToEventSourcing> logger,
        Model writeModel, EventStoreService eventStoreService, ClientCollection clientCollection)
    {
        _logger = logger;
        _writeModel = writeModel;
        _eventStoreService = eventStoreService;
        _clientCollection = clientCollection;
    }
}