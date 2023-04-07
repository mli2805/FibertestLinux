using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

[ApiController]
[Route("[controller]")]
public class MeasurementController : ControllerBase
{
    private readonly ILogger<MiscController> _logger;
    private readonly C2DCommandsProcessor _c2DCommandsProcessor;
    private readonly string _localIpAddress;

    public MeasurementController(IWritableConfig<DataCenterConfig> config, ILogger<MiscController> logger, C2DCommandsProcessor c2DCommandsProcessor)
    {
        _logger = logger;
        _c2DCommandsProcessor = c2DCommandsProcessor;
        _localIpAddress = config.Value.General.ServerDoubleAddress.Main.Ip4Address;
    }

    private string GetRemoteAddress()
    {
        var ip1 = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        // browser started on the same pc as this service
        return ip1 == "::1" || ip1 == "127.0.0.1" ? _localIpAddress : ip1;
    }

    [Authorize]
    [HttpPost("Update")]
    public async Task<RequestAnswer> AttachTrace()
    {
        string body;
        using (var reader = new StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }
        _logger.Info(Logs.WebApi, $"{body}");
        var dto = JsonConvert.DeserializeObject<UpdateMeasurementDto>(body);
        if (dto == null)
            return new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = "Failed to deserialize json" };

        dto.ClientIp = GetRemoteAddress();
        dto.StatusChangedTimestamp = DateTime.Now;
        dto.StatusChangedByUser = User.Identity!.Name;

        var result = await _c2DCommandsProcessor.UpdateMeasurement(dto);
        return new RequestAnswer(result == null ? ReturnCode.Ok : ReturnCode.Error);
    }
}