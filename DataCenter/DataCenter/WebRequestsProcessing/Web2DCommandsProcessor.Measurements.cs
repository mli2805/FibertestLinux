using Fibertest.Dto;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public partial class Web2DCommandsProcessor
    {
        public async Task<ClientMeasurementStartedDto> StartMeasurementClient(DoClientMeasurementDto dto)
        {
            var response = await _c2RCommandsProcessor.SendCommand(dto);
            return (ClientMeasurementStartedDto)JsonConvert.DeserializeObject(response, JsonSerializerSettings)!;
        }

        public async Task<RequestAnswer> OutOfTurnPreciseMeasurement(DoOutOfTurnPreciseMeasurementDto dto)
        {
            var response = await _c2RCommandsProcessor.SendCommand(dto);
            return (RequestAnswer)JsonConvert.DeserializeObject(response, JsonSerializerSettings)!;
        }
    }
}
