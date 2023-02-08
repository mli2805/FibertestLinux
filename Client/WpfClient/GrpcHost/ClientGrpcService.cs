using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient;

public class ClientGrpcService
{
    private readonly IWritableConfig<ClientConfig> _config;
    private readonly ILogger _logger;

    public ClientGrpcService(IWritableConfig<ClientConfig> config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }
}