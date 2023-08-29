using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fibertest.DataCenter;

[ApiController]
[Route("[controller]")]
public class SorController : ControllerBase
{
    private readonly IWritableConfig<DataCenterConfig> _config;
    private readonly ILogger<SorController> _logger;

    public SorController(IWritableConfig<DataCenterConfig> config, ILogger<SorController> logger)
    {
        _config = config;
        _logger = logger;
    }
}