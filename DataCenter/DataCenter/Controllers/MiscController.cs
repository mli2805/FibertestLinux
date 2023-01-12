using Fibertest.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fibertest.DataCenter;

[ApiController]
[Route("[controller]")]
public class MiscController : ControllerBase
{
    private readonly ILogger<MiscController> _logger;

    public MiscController(ILogger<MiscController> logger)
    {
        _logger = logger;
    }

    [HttpGet("CheckApi")]
    public string CheckApi()
    {
        _logger.LLog(Logs.DataCenter, "MiscController CheckApi");
        return "Fibertest 3.0 controller greets you!";
    }
}