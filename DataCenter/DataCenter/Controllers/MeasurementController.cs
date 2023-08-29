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
    private readonly ILogger<MeasurementController> _logger;
    private readonly C2DCommandsProcessor _c2DCommandsProcessor;
    private readonly Web2DCommandsProcessor _web2DCommandsProcessor;
    private readonly string _localIpAddress;

    public MeasurementController(IWritableConfig<DataCenterConfig> config, ILogger<MeasurementController> logger,
        C2DCommandsProcessor c2DCommandsProcessor, Web2DCommandsProcessor web2DCommandsProcessor)
    {
        _logger = logger;
        _c2DCommandsProcessor = c2DCommandsProcessor;
        _web2DCommandsProcessor = web2DCommandsProcessor;
        _localIpAddress = config.Value.General.ServerDoubleAddress.Main.Ip4Address;
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

        dto.ClientIp = HttpContext.GetRemoteAddress(_localIpAddress);
        dto.StatusChangedTimestamp = DateTime.Now;
        dto.StatusChangedByUser = User.Identity!.Name;

        var result = await _c2DCommandsProcessor.UpdateMeasurement(dto);
        return new RequestAnswer(result == null ? ReturnCode.Ok : ReturnCode.Error);
    }

    [Authorize]
    [HttpPost("Start-measurement-client")]
    public async Task<ClientMeasurementStartedDto> StartMeasurementClient()
    {
        _logger.Info(Logs.WebApi,
            $"MeasurementClient request from {HttpContext.GetRemoteAddress(_localIpAddress)}");

        try
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            _logger.Info(Logs.WebApi, $"{body}");

            var dto = JsonConvert.DeserializeObject<DoClientMeasurementDto>(body);
            var clientMeasurementStartedDto = await _web2DCommandsProcessor.StartMeasurementClient(dto);
            return clientMeasurementStartedDto;
        }
        catch (Exception e)
        {
            _logger.Info(Logs.WebApi, $"StartMeasurementClient: {e.Message}");
            return new ClientMeasurementStartedDto() 
                { ErrorMessage = e.Message, ReturnCode = ReturnCode.MeasurementPreparationError };
        }
    }

    [Authorize]
    [HttpGet("Get-measurement-client-result")]
    public async Task<ClientMeasurementVeexResultDto> 
        GetVeexMeasurementClientResult(string connectionId, string rtuId, string veexMeasurementId)
    {
        _logger.Info(Logs.WebApi,
            $"Get Veex MeasurementClient result request from {HttpContext.GetRemoteAddress(_localIpAddress)}");

        try
        {
            var _ = new GetClientMeasurementDto(Guid.Parse(rtuId), RtuMaker.VeEX)
            { ClientConnectionId = connectionId, VeexMeasurementId = veexMeasurementId };

            return new ClientMeasurementVeexResultDto() {ReturnCode = ReturnCode.NotImplementedYet};
        }
        catch (Exception e)
        {
            _logger.Info(Logs.WebApi, $"StartMeasurementClient: {e.Message}");
            return new ClientMeasurementVeexResultDto() { ErrorMessage = e.Message, ReturnCode = ReturnCode.Error };
        }
    }

    [Authorize]
    [HttpPost("Out-of-turn-measurement")]
    public async Task<RequestAnswer> OutOfTurnPreciseMeasurement()
    {
        _logger.Info(Logs.WebApi,
            $"OutOfTurnPreciseMeasurement request from {HttpContext.GetRemoteAddress(_localIpAddress)}");

        try
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            var dto = JsonConvert.DeserializeObject<DoOutOfTurnPreciseMeasurementDto>(body);
            var outOfTurnMeasurementStarted = await _web2DCommandsProcessor.OutOfTurnPreciseMeasurement(dto!);
            return outOfTurnMeasurementStarted;
        }
        catch (Exception e)
        {
            _logger.Info(Logs.WebApi, $"OutOfTurnPreciseMeasurement: {e.Message}");
            return new RequestAnswer() { ErrorMessage = e.Message, ReturnCode = ReturnCode.MeasurementPreparationError };
        }
    }
}