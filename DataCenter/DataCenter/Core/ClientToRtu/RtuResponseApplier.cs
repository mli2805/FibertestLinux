using Fibertest.Dto;
using Fibertest.Graph;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public partial class RtuResponseApplier
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    private readonly ILogger<RtuResponseApplier> _logger;
    private readonly Model _writeModel;
    private readonly SorFileRepository _sorFileRepository;
    private readonly RtuStationsRepository _rtuStationsRepository;
    private readonly RtuResponseToEventSourcing _responseToEventSourcing;
    private readonly IFtSignalRClient _ftSignalRClient;

    public RtuResponseApplier(ILogger<RtuResponseApplier> logger, Model writeModel,
        SorFileRepository sorFileRepository, RtuStationsRepository rtuStationsRepository,
        RtuResponseToEventSourcing responseToEventSourcing, IFtSignalRClient ftSignalRClient)
    {
        _logger = logger;
        _writeModel = writeModel;
        _sorFileRepository = sorFileRepository;
        _rtuStationsRepository = rtuStationsRepository;
        _responseToEventSourcing = responseToEventSourcing;
        _ftSignalRClient = ftSignalRClient;
    }

  
    private TResult Deserialize<TResult>(string jsonResult) where TResult : RequestAnswer, new()
    {
        var result = JsonConvert.DeserializeObject<TResult>(jsonResult, JsonSerializerSettings);
        if (result != null) return result;

        // if problems during connection result could be of type <RequestAnswer>
        var answer = JsonConvert.DeserializeObject<RequestAnswer>(jsonResult, JsonSerializerSettings);
        return answer == null
            ? new TResult() { ReturnCode = ReturnCode.FailedDeserializeJson }
            : new TResult() { ReturnCode = answer.ReturnCode, ErrorMessage = answer.ErrorMessage };
    }
}