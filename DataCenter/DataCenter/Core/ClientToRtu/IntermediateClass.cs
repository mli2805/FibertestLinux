using Fibertest.Dto;
using Fibertest.Utils;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public class IntermediateClass
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        private readonly ILogger<IntermediateClass> _logger;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly ClientToIitRtuTransmitter _clientToIitRtuTransmitter;
        private readonly RtuInitializationToGraphApplier _rtuInitializationToGraphApplier;

        private readonly DoubleAddress _serverDoubleAddress;


        public IntermediateClass(IWritableOptions<DataCenterConfig> config, ILogger<IntermediateClass> logger, 
            RtuStationsRepository rtuStationsRepository, ClientToIitRtuTransmitter clientToIitRtuTransmitter,
            RtuInitializationToGraphApplier rtuInitializationToGraphApplier)
        {
            _logger = logger;
            _rtuStationsRepository = rtuStationsRepository;
            _clientToIitRtuTransmitter = clientToIitRtuTransmitter;
            _rtuInitializationToGraphApplier = rtuInitializationToGraphApplier;

            _serverDoubleAddress = config.Value.General.ServerDoubleAddress;
        }

        // Web and Desktop clients send different dtos for RTU initialization
        // so Grpc service and Http controller call this function to send command to RTU
        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            dto.ServerAddresses = _serverDoubleAddress.Clone();
            if (!dto.RtuAddresses.HasReserveAddress)
                // if RTU has no reserve address it should not send to server's reserve address
                // (it is an ideological requirement)
                dto.ServerAddresses.HasReserveAddress = false;

            var json = dto.RtuMaker == RtuMaker.IIT
                ? await _clientToIitRtuTransmitter
                    .TransferCommand(dto.RtuAddresses.Main.ToStringA(), JsonConvert.SerializeObject(dto, JsonSerializerSettings))
                : JsonConvert.SerializeObject(new RequestAnswer(ReturnCode.NotImplementedYet), JsonSerializerSettings);

            var res = JsonConvert.DeserializeObject<RtuInitializedDto>(json, JsonSerializerSettings);
            if (res != null)
                return await ProcessInitializationResult(dto, res);

            var answer = JsonConvert.DeserializeObject<RequestAnswer>(json, JsonSerializerSettings);
            return answer != null 
                ? new RtuInitializedDto(answer.ReturnCode) { ErrorMessage = answer.ErrorMessage }
                : new RtuInitializedDto(ReturnCode.FailedDeserializeJson);
        }

        private async Task<RtuInitializedDto> ProcessInitializationResult(InitializeRtuDto dto, RtuInitializedDto result)
        {
            var message = result.IsInitialized
                ? "RTU initialized successfully."
                : "RTU initialization failed";
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), message);
            
            //await _ftSignalRClient.NotifyAll("RtuInitialized", rtuInitializedDto.ToCamelCaseJson());

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
            }

            await _rtuInitializationToGraphApplier.ApplyRtuInitializationResult(dto, result);
            return result;
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
    }
}
