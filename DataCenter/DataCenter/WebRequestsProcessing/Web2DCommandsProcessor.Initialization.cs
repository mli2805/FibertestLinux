using Fibertest.Dto;
using Fibertest.Utils;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public partial class Web2DCommandsProcessor
    {
        /// <summary>
        /// not the same as desktop command:
        /// web client sends only id of RTU which had already been initialized and now should be RE-initialized
        /// </summary>
        /// <param name="dto">contains only RTU ID and will be filled in now (on data center)</param>
        /// <returns></returns>
        public async Task<RtuInitializedWebDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            if (!FillIn(dto))
                return new RtuInitializedWebDto() { ReturnCode = ReturnCode.RtuInitializationError, };
            var response = await _c2RCommandsProcessor.SendCommand(dto);
            var resultDto = (RtuInitializedDto)JsonConvert.DeserializeObject(response)!;
            if (resultDto.ReturnCode == ReturnCode.Ok)
                resultDto.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
            _logger.Info(Logs.WebApi, $"Rtu initialization: {resultDto.ReturnCode}");
            return Map(resultDto);
        }

        private bool FillIn(InitializeRtuDto dto)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return false;
            dto.RtuMaker = rtu.RtuMaker;
            dto.RtuAddresses = new DoubleAddress()
            {
                Main = rtu.MainChannel.Clone(),
                Reserve = rtu.ReserveChannel.Clone(),
                HasReserveAddress = rtu.IsReserveChannelSet,
            };
            dto.IsFirstInitialization = false;
            dto.Serial = rtu.Serial;
            dto.OwnPortCount = rtu.OwnPortCount;
            dto.Children = rtu.Children;
            return true;
        }

        private RtuInitializedWebDto Map(RtuInitializedDto dto)
        {
            return new RtuInitializedWebDto()
            {
                RtuId = dto.RtuId,
                ReturnCode = dto.ReturnCode,
                ErrorMessage = dto.ErrorMessage,
                RtuNetworkSettings = new RtuNetworkSettingsDto()
                {
                    MainChannel = dto.RtuAddresses.Main.ToStringASpace(),
                    IsReserveChannelSet = dto.RtuAddresses.HasReserveAddress,
                    ReserveChannel = dto.RtuAddresses.Reserve?.ToStringASpace(),
                    OtdrAddress = dto.OtdrAddress == null
                        ? ""
                        : dto.OtdrAddress.Ip4Address == "192.168.88.101"
                            ? $"{dto.RtuAddresses.Main.Ip4Address} : {dto.OtdrAddress.Port}"
                            : dto.OtdrAddress.ToStringASpace(),
                    Mfid = dto.Mfid,
                    Serial = dto.Serial,
                    OwnPortCount = dto.OwnPortCount,
                    FullPortCount = dto.FullPortCount,
                    Version = dto.Version,
                    Version2 = dto.Version2,
                }
            };
        }
    }
}
