using Fibertest.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fibertest.Rtu
{
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
            _logger.Info(Logs.RtuService, "MiscController CheckApi");
            return "Fibertest Linux RTU http controller greets you!";
        }
    }
}
