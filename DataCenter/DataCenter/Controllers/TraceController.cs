using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fibertest.DataCenter;

[ApiController]
[Route("[controller]")]
public class TraceController : ControllerBase
{
    private readonly IWritableConfig<DataCenterConfig> _config;
    private readonly ILogger<TraceController> _logger;

    public TraceController(IWritableConfig<DataCenterConfig> config, ILogger<TraceController> logger)
    {
        _config = config;
        _logger = logger;
    }
}