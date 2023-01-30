using System.Collections.Generic;
using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.OtdrDataFormat;
using Fibertest.Utils;

namespace Fibertest.WpfClient
{
    public class MeasurementAsBaseAssigner
    {
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;

        private readonly CurrentUser _currentUser;
        private Rtu _rtu;

        public MeasurementAsBaseAssigner(CurrentUser currentUser, IWcfServiceCommonC2D c2DWcfCommonManager)
        {
            _currentUser = currentUser;
            _c2DWcfCommonManager = c2DWcfCommonManager;
        }

        public void Initialize(Rtu rtu)
        {
            _rtu = rtu;
        }

        public async Task<BaseRefAssignedDto> Assign(OtdrDataKnownBlocks sorData, Trace trace)
        {
            var dto = PrepareDto(sorData.ToBytes(), trace);
            return await _c2DWcfCommonManager.AssignBaseRefAsync(dto); // send to Db and RTU
        }

        private AssignBaseRefsDto PrepareDto(byte[] sorBytes, Trace trace)
        {
            var dto = new AssignBaseRefsDto(trace.RtuId, _rtu.RtuMaker, trace.TraceId, new List<BaseRefDto>(), new List<int>())
            {
                OtdrId = _rtu.OtdrId,
                OtauPortDto = trace.OtauPort,
            };

            if (trace.OtauPort != null && !trace.OtauPort.IsPortOnMainCharon && _rtu.RtuMaker == RtuMaker.VeEX)
            {
                dto.MainOtauPortDto = new OtauPortDto(trace.OtauPort.MainCharonPort, true)
                {
                    OtauId = _rtu.MainVeexOtau.id,
                };
            }

            dto.BaseRefs = new List<BaseRefDto>()
            {
                BaseRefDtoFactory.CreateFromBytes(BaseRefType.Precise, sorBytes, _currentUser.UserName),
                BaseRefDtoFactory.CreateFromBytes(BaseRefType.Fast, sorBytes, _currentUser.UserName)
            };
            return dto;
        }
    }
}
