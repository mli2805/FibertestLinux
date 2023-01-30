using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public static class ReSendDtoExt
    {
        public static IEnumerable<ReSendBaseRefsDto> CreateReSendDtos(this Model model, Rtu rtu, CurrentUser currentUser)
        {
            foreach (var trace in model.Traces
                         .Where(t => t.RtuId == rtu.Id && t.IsAttached && t.HasAnyBaseRef))
            {
                var dto = new ReSendBaseRefsDto(rtu.Id, rtu.RtuMaker)
                {
                    OtdrId = rtu.OtdrId,
                    TraceId = trace.TraceId,
                    OtauPortDto = trace.OtauPort,
                    BaseRefDtos = new List<BaseRefDto>(),
                };
                foreach (var baseRef in model.BaseRefs.Where(b => b.TraceId == trace.TraceId))
                {
                    dto.BaseRefDtos.Add(new BaseRefDto()
                    {
                        SorFileId = baseRef.SorFileId,

                        Id = baseRef.TraceId,
                        BaseRefType = baseRef.BaseRefType,
                        Duration = baseRef.Duration,
                        SaveTimestamp = baseRef.SaveTimestamp,
                        UserName = baseRef.UserName,
                    });
                }
                yield return dto;
            }
        }
    }
}