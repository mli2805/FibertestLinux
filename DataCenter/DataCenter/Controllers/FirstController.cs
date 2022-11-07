using Fibertest.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Fibertest.DataCenter
{
    [ApiController]
    [Route("[controller]")]
    public class FirstController : ControllerBase
    {
        private readonly ILogger<FirstController> _logger;

        public FirstController(ILogger<FirstController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), null, "FirstController Get");
            return "First controller greets you!";
        }
    }
}
