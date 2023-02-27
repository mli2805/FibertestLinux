using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

[ApiController]
[Route("[controller]")]
public class PortController : ControllerBase
{
    private readonly ILogger<MiscController> _logger;
    private readonly C2RCommandsProcessor _c2RCommandsProcessor;
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    public PortController(ILogger<MiscController> logger, C2RCommandsProcessor c2RCommandsProcessor)
    {
        _logger = logger;
        _c2RCommandsProcessor = c2RCommandsProcessor;
    }

    [Authorize]
    [HttpPost("Attach-trace")]
    public async Task<RequestAnswer> AttachTrace()
    {
        try
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            _logger.LogInfo(Logs.WebApi, body);
            //TODO: de-serialization from WEB Client , check JsonSerializerSettings
            var dto = JsonConvert.DeserializeObject<AttachTraceDto>(body);
            if (dto == null)
                return new RequestAnswer(ReturnCode.FailedDeserializeJson);

            var jsonResult = await _c2RCommandsProcessor.SendCommand(dto);
            var result = JsonConvert.DeserializeObject<RequestAnswer>(jsonResult, JsonSerializerSettings);

            if (result == null)
                return new RequestAnswer(ReturnCode.FailedDeserializeJson);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.WebApi, $"AttachTrace: {e.Message}");
            return new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = e.Message };
        }
    }
}