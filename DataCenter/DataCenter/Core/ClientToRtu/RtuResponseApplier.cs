using Fibertest.Dto;
using Fibertest.Utils;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public class RtuResponseApplier
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    private readonly ILogger<IntermediateClass> _logger;
    private readonly RtuStationsRepository _rtuStationsRepository;
    private readonly RtuResponseToGraphApplier _responseToGraphApplier;
    private readonly IFtSignalRClient _ftSignalRClient;

    public RtuResponseApplier(ILogger<IntermediateClass> logger, RtuStationsRepository rtuStationsRepository,
        RtuResponseToGraphApplier responseToGraphApplier, IFtSignalRClient ftSignalRClient)
    {
        _logger = logger;
        _rtuStationsRepository = rtuStationsRepository;
        _responseToGraphApplier = responseToGraphApplier;
        _ftSignalRClient = ftSignalRClient;
    }

    public async Task<string> ApplyOtauAttachmentResult(AttachOtauDto dto, string jsonResult)
    {
        return jsonResult;
    }

    public async Task<string> ApplyRtuInitializationResult(InitializeRtuDto dto, string jsonResult)
    {
        var result = Deserialize<RtuInitializedDto>(jsonResult);
      
        var message = result.IsInitialized
            ? "RTU initialized successfully."
            : "RTU initialization failed";
        _logger.LLog(Logs.DataCenter.ToInt(), message);

        await _ftSignalRClient.NotifyAll("RtuInitialized", result.ToCamelCaseJson());

        if (result.IsInitialized)
        {
            try
            {
                result.RtuAddresses = dto.RtuAddresses;
                var rtuStation = CreateRtuStation(result);
                await _rtuStationsRepository.RegisterRtuInitializationResultAsync(rtuStation);
            }
            catch (Exception e)
            {
                result.ReturnCode = ReturnCode.Error;
                result.ErrorMessage = $"Failed to save RTU in DB: {e.Message}";
            }

            await _responseToGraphApplier.ApplyRtuInitializationResult(dto, result);
        }

        return jsonResult;
    }

    private static RtuStation CreateRtuStation(RtuInitializedDto dto)
    {
        var rtuStation = new RtuStation()
        {
            RtuGuid = dto.RtuId,
            Version = dto.Version ?? "unknown",
            MainAddress = dto.RtuAddresses.Main.GetAddress(),
            MainAddressPort = dto.RtuAddresses.Main.Port,
            LastConnectionByMainAddressTimestamp = DateTime.Now,
            IsMainAddressOkDuePreviousCheck = true,
            IsReserveAddressSet = dto.RtuAddresses.HasReserveAddress,
            LastMeasurementTimestamp = DateTime.Now,
        };
        if (dto.RtuAddresses.HasReserveAddress)
        {
            rtuStation.ReserveAddress = dto.RtuAddresses.Reserve.GetAddress();
            rtuStation.ReserveAddressPort = dto.RtuAddresses.Reserve.Port;
            rtuStation.LastConnectionByReserveAddressTimestamp = DateTime.Now;
        }
        return rtuStation;
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