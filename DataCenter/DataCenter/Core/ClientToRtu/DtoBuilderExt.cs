using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter
{
    public static class DtoBuilderExt
    {
        public static async Task<AssignBaseRefsDto?> CreateAssignBaseRefsDto(
            this Model writeModel, AttachTraceDto cmd, SorFileRepository sorFileRepository)
        {
            var trace = writeModel.Traces.FirstOrDefault(t => t.TraceId == cmd.TraceId);
            if (trace == null) return null;
            var rtu = writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null) return null;

            var dto = new AssignBaseRefsDto(rtu.Id, rtu.RtuMaker, 
                cmd.TraceId, new List<BaseRefDto>(), new List<int>())
            {
                OtdrId = rtu.OtdrId!,
                OtauPortDto = cmd.OtauPortDto,
                MainOtauPortDto = cmd.MainOtauPortDto,
                BaseRefs = new List<BaseRefDto>(),
            };

            foreach (var baseRef in writeModel.BaseRefs.Where(b => b.TraceId == trace.TraceId))
            {
                dto.BaseRefs.Add(new BaseRefDto()
                {
                    SorFileId = baseRef.SorFileId,
                    Id = baseRef.TraceId,
                    BaseRefType = baseRef.BaseRefType,
                    Duration = baseRef.Duration,
                    SaveTimestamp = baseRef.SaveTimestamp,
                    UserName = baseRef.UserName,

                    SorBytes = await sorFileRepository.GetSorBytesAsync(baseRef.SorFileId),
                });
            }

            return dto;
        }
    }
}
