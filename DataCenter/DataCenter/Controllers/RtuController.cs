using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

[ApiController]
[Route("[controller]")]
public class RtuController : ControllerBase
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private readonly ILogger<RtuController> _logger;
    private readonly Web2DCommandsProcessor _web2DCommandsProcessor;
    private readonly string _localIpAddress;

    public RtuController(IWritableConfig<DataCenterConfig> config, ILogger<RtuController> logger,
        Web2DCommandsProcessor web2DCommandsProcessor)
    {
        _logger = logger;
        _localIpAddress = config.Value.General.ServerDoubleAddress.Main.Ip4Address;
        _web2DCommandsProcessor = web2DCommandsProcessor;
    }

    [Authorize]
    [HttpGet("Tree")]
    public async Task<IEnumerable<RtuDto>> GetTree(string connectionId, string username)
    {
        _logger.Info(Logs.WebApi, $"GetTree requested by {username} / {connectionId}");
        var tree = await _web2DCommandsProcessor
            .GetTreeInJson(User.Identity.Name);
        if (tree == null)
        {
            _logger.Info(Logs.WebApi, "Failed to get tree");
            return null;
        }
        _logger.Debug(Logs.WebApi, tree);

        var rtuList = (List<RtuDto>)JsonConvert.DeserializeObject(tree, JsonSerializerSettings);
        _logger.Info(Logs.WebApi, rtuList == null
            ? "Failed to deserialize RTU list"
            : $"RTU list contains {rtuList.Count} items");


        return rtuList;
    }

    [Authorize]
    [HttpGet("Information/{id}")]
    public async Task<RtuInformationDto> GetRtuInformation(string id)
    {
        try
        {
            _logger.Info(Logs.WebApi, $"rtu id = {id}");
            var rtuGuid = Guid.Parse(id);
            var rtuInformationDto = await _web2DCommandsProcessor
                .GetRtuInformation(User.Identity.Name, rtuGuid);
            _logger.Info(Logs.WebApi, rtuInformationDto == null
                ? "Failed to get RTU's information"
                : "RTU information ");
            return rtuInformationDto;
        }
        catch (Exception e)
        {
            _logger.Info(Logs.WebApi, $"GetRtuInformation: {e.Message}");
        }
        return null;
    }

    [Authorize]
    [HttpGet("Network-settings/{id}")]
    public async Task<RtuNetworkSettingsDto> GetRtuNetworkSettings(string id)
    {
        try
        {
            _logger.Info(Logs.WebApi, $"rtu id = {id}");
            var rtuGuid = Guid.Parse(id);
            var rtuNetworkSettingsDto = await _web2DCommandsProcessor
                .GetRtuNetworkSettings(User.Identity.Name, rtuGuid);
            _logger.Info(Logs.WebApi, rtuNetworkSettingsDto == null
                ? "Failed to get RTU's network-settings"
                : "RTU Network-settings ");
            return rtuNetworkSettingsDto;
        }
        catch (Exception e)
        {
            _logger.Info(Logs.WebApi, $"GetRtuNetworkSettings: {e.Message}");
        }
        return null;
    }

    [Authorize]
    [HttpGet("State/{id}")]
    public async Task<RtuStateDto> GetRtuState(string id)
    {
        try
        {
            _logger.Info(Logs.WebApi, $"rtu id = {id}");
            var rtuGuid = Guid.Parse(id);
            var rtuStateDto = await _web2DCommandsProcessor
                .GetRtuState(User.Identity.Name, rtuGuid);
            _logger.Info(Logs.WebApi, rtuStateDto == null
                ? "Failed to get RTU's state"
                : "RTU state ");
            return rtuStateDto;
        }
        catch (Exception e)
        {
            _logger.Info(Logs.WebApi, $"GetRtuState: {e.Message}");
        }
        return null;
    }

    [Authorize]
    [HttpGet("Measurement-parameters/{id}")]
    public async Task<TreeOfAcceptableMeasParams?> GetRtuAcceptableMeasParams(string id)
    {
        try
        {
            _logger.Info(Logs.WebApi, $"rtu id = {id}");
            var rtuGuid = Guid.Parse(id);
            var rtuMeasParams = await _web2DCommandsProcessor
                .GetRtuAcceptableMeasParams(User.Identity.Name, rtuGuid);
            _logger.Info(Logs.WebApi, rtuMeasParams == null
                ? "Failed to GetRtuAcceptableMeasParams"
                : "RTU acceptable meas params ");
            return rtuMeasParams;
        }
        catch (Exception e)
        {
            _logger.Exception(Logs.WebApi, e, $"GetRtuAcceptableMeasParams: ");
        }
        return null;
    }

    [Authorize]
    [HttpGet("Monitoring-settings/{id}")]
    public async Task<RtuMonitoringSettingsDto> GetRtuMonitoringSettings(string id)
    {
        try
        {
            _logger.Info(Logs.WebApi, $"rtu id = {id}");
            var rtuGuid = Guid.Parse(id);
            var rtuMonitoringSettingsDto = await _web2DCommandsProcessor
                .GetRtuMonitoringSettings(User.Identity.Name, rtuGuid);
            _logger.Info(Logs.WebApi, rtuMonitoringSettingsDto == null
                ? "Failed to get RTU's Monitoring-settings"
                : "RTU Monitoring-settings ");
            return rtuMonitoringSettingsDto;
        }
        catch (Exception e)
        {
            _logger.Info(Logs.WebApi, $"GetRtuMonitoringSettings: {e.Message}");
        }
        return null;
    }

    [Authorize]
    [HttpPost("Monitoring-settings/{id}")]
    public async Task<RequestAnswer> PostRtuMonitoringSettings(string id)
    {
        try
        {
            _logger.Info(Logs.WebApi, $"rtu id = {id}");
            var rtuGuid = Guid.Parse(id);
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            _logger.Info(Logs.WebApi, body);
            var dto = JsonConvert.DeserializeObject<RtuMonitoringSettingsDto>(body);
            var applyDto = Map(rtuGuid, dto);
            var monitoringSettingsAppliedDto = await _web2DCommandsProcessor
                .ApplyMonitoringSettingsAsync(applyDto);
            _logger.Info(Logs.WebApi, $"PostRtuMonitoringSettings: {monitoringSettingsAppliedDto.ReturnCode.ToString()}");
            return monitoringSettingsAppliedDto;
        }
        catch (Exception e)
        {
            _logger.Info(Logs.WebApi, $"PostRtuMonitoringSettings: {e.Message}");
            return new RequestAnswer() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError, ErrorMessage = e.Message };
        }
    }

    private ApplyMonitoringSettingsDto Map(Guid rtuId, RtuMonitoringSettingsDto dto)
    {
        var applyMonitoringSettingsDto = new ApplyMonitoringSettingsDto(rtuId, dto.RtuMaker)
        {
            ClientConnectionId = dto.ConnectionId ?? "",
            OtdrId = dto.OtdrId,
            MainVeexOtau = dto.MainVeexOtau,
            IsMonitoringOn = dto.MonitoringMode == MonitoringState.On,

            Timespans = new MonitoringTimespansDto()
            {
                FastSave = dto.FastSave.GetTimeSpan(),
                PreciseMeas = dto.PreciseMeas.GetTimeSpan(),
                PreciseSave = dto.PreciseSave.GetTimeSpan(),
            },

            Ports = new List<PortWithTraceDto>(),
        };
        foreach (var line in dto.Lines!
                     .Where(l => l.PortMonitoringMode == PortMonitoringMode.On))
        {
            var traceGuid = Guid.Parse(line.TraceId!);
            var portWithTraceDto = new PortWithTraceDto(line.OtauPortDto!, traceGuid);
            applyMonitoringSettingsDto.Ports.Add(portWithTraceDto);
        }

        return applyMonitoringSettingsDto;
    }

    [Authorize]
    [HttpPost("Set-rtu-occupation-state")]
    public async Task<RequestAnswer> SetRtuOccupationState()
    {
        string body;
        using (var reader = new StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }
        _logger.Info(Logs.WebApi, "body: " + body);
        var dto = JsonConvert.DeserializeObject<OccupyRtuDto>(body);

        var res = await _web2DCommandsProcessor.SetRtuOccupationState(dto!);
        return res;
    }

    [Authorize]
    [HttpPost("Stop-monitoring")]
    public async Task<bool> StopMonitoring()
    {
        try
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            _logger.Info(Logs.WebApi, $"{body}");
            var dto = JsonConvert.DeserializeObject<StopMonitoringDto>(body);
            var isStopped = await _web2DCommandsProcessor.StopMonitoringAsync(dto!);
            return isStopped;
        }
        catch (Exception e)
        {
            _logger.Info(Logs.WebApi, $"StopMonitoring: {e.Message}");
            return false;
        }
    }

    [Authorize]
    [HttpPost("Initialize")]
    public async Task<RtuInitializedWebDto> InitializeRtu()
    {
        _logger.Info(Logs.WebApi,
            $"RTU initialization request from {HttpContext.GetRemoteAddress(_localIpAddress)}");

        try
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            _logger.Info(Logs.WebApi, $"{body}");

            var dto = JsonConvert.DeserializeObject<InitializeRtuDto>(body);
            return await _web2DCommandsProcessor.InitializeRtuAsync(dto!);
        }
        catch (Exception e)
        {
            _logger.Info(Logs.WebApi, $"InitializeRtu: {e.Message}");
            return new RtuInitializedWebDto();
        }
    }
}