using Fibertest.Dto;
using Fibertest.Utils;
using Newtonsoft.Json;

namespace Fibertest.DataCenter;

public partial class RtuResponseApplier
{
    public async Task<string> ApplyRtuInitializationResult(InitializeRtuDto dto, string jsonResult)
    {
        var result = Deserialize<RtuInitializedDto>(jsonResult);

        if (result.IsInitialized)
        {
            try
            {
                result.RtuAddresses = dto.RtuAddresses;
                var rtuStation = CreateRtuStation(result);
                await _rtuStationsRepository.RegisterRtuInitializationResultAsync(rtuStation);
                await _ftSignalRClient.NotifyAll("RtuInitialized", result.ToCamelCaseJson());
            }
            catch (Exception e)
            {
                result.ReturnCode = ReturnCode.Error;
                result.ErrorMessage = $"Failed to save RTU in DB: {e.Message}";
            }

            await _responseToEventSourcing.ApplyRtuInitializationResult(dto, result);
            _logger.Info(Logs.DataCenter, "RTU initialized successfully.");
        }
        else
            _logger.Error(Logs.DataCenter, "Failed to initialize RTU!");

        return JsonConvert.SerializeObject(result, JsonSerializerSettings);
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
            rtuStation.ReserveAddress = dto.RtuAddresses.Reserve!.GetAddress();
            rtuStation.ReserveAddressPort = dto.RtuAddresses.Reserve.Port;
            rtuStation.LastConnectionByReserveAddressTimestamp = DateTime.Now;
        }
        return rtuStation;
    }
}