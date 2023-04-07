using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter
{
    public partial class C2DCommandsProcessor
    {
        public async Task<string?> UpdateMeasurement(UpdateMeasurementDto dto)
        {
            var cmd = Mapper.Map<UpdateMeasurement>(dto);
            return await _eventStoreService.SendCommand(cmd, dto.StatusChangedByUser!, dto.ClientIp!);
        }

    }
}
