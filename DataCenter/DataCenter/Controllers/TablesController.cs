using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fibertest.DataCenter;

[ApiController]
[Route("[controller]")]
public class TablesController : ControllerBase
{
    private readonly IWritableConfig<DataCenterConfig> _config;
    private readonly ILogger<TablesController> _logger;
    private readonly Web2DCommandsProcessor _web2DCommandsProcessor;

    public TablesController(IWritableConfig<DataCenterConfig> config, ILogger<TablesController> logger,
        Web2DCommandsProcessor web2DCommandsProcessor)
    {
        _config = config;
        _logger = logger;
        _web2DCommandsProcessor = web2DCommandsProcessor;
    }

    [Authorize]
    [HttpGet("GetOpticalsPage")]
    public async Task<OpticalEventsRequestedDto> GetOpticalsPage(bool isCurrentEvents,
                 string filterRtu, string filterTrace, string sortOrder, int pageNumber, int pageSize)
    {
        var resultDto = await _web2DCommandsProcessor
            .GetOpticalEventPortion(User.Identity.Name, isCurrentEvents, filterRtu,
                filterTrace, sortOrder, pageNumber, pageSize);
        _logger.Info(Logs.WebApi, resultDto == null
            ? "Failed to get optical event list"
            : $"Optical event list contains {resultDto.FullCount} items");

        return resultDto;
    }

    [Authorize]
    [HttpGet("GetNetworksPage")]
    public async Task<NetworkEventsRequestedDto> GetNetworksPage(bool isCurrentEvents,
                      string filterRtu, string sortOrder, int pageNumber, int pageSize)
    {
        var resultDto = await _web2DCommandsProcessor
                .GetNetworkEventPortion(User.Identity.Name, isCurrentEvents, filterRtu, sortOrder, pageNumber, pageSize);
        _logger.Info(Logs.WebApi, resultDto == null
            ? "Failed to get optical event list"
            : $"Optical event list contains {resultDto.FullCount} items");

        return resultDto;
    }

    [Authorize]
    [HttpGet("GetBopsPage")]
    public async Task<BopEventsRequestedDto> GetBopsPage(bool isCurrentEvents,
                            string filterRtu, string sortOrder, int pageNumber, int pageSize)
    {
        var resultDto = await _web2DCommandsProcessor
                .GetBopEventPortion(User.Identity.Name, isCurrentEvents, filterRtu, sortOrder, pageNumber, pageSize);
        _logger.Info(Logs.WebApi, resultDto == null
            ? "Failed to get bop event list"
            : $"Bop event list contains {resultDto.FullCount} items");

        return resultDto;
    }
}