using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public partial class RtuResponseApplier
    {
        public async Task<string> ApplyBaseRefsAssignmentResult(AttachTraceDto dto, string jsonResult)
        {
            var result = Deserialize<BaseRefAssignedDto>(jsonResult);
            if (result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully)
            {
                var commandForEventSourcing = new AttachTrace(dto.TraceId, dto.OtauPortDto!);
                await _responseToEventSourcing.ApplyBaseSendingResult(dto, commandForEventSourcing);
            }
            else
                _logger.LogError(Logs.DataCenter, "Failed to assign base refs!");

            return jsonResult;
        }

        public async Task<string> ApplyBaseRefsAssignmentResult(AssignBaseRefsDto dto, string jsonResult)
        {
            var result = Deserialize<BaseRefAssignedDto>(jsonResult);
            if (result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully)
            {
                var commandForEventSourcing = await SaveSorInDb(dto);
                await _responseToEventSourcing.ApplyBaseSendingResult(dto, commandForEventSourcing);
            }
            else
                _logger.LogError(Logs.DataCenter, "Failed to assign base refs!");

            return jsonResult;
        }

        private async Task<AssignBaseRef> SaveSorInDb(AssignBaseRefsDto dto)
        {
            var command = new AssignBaseRef { TraceId = dto.TraceId, BaseRefs = new List<BaseRef>() };
            foreach (var baseRefDto in dto.BaseRefs)
            {
                var oldBaseRef = _writeModel.BaseRefs.FirstOrDefault(b =>
                    b.TraceId == dto.TraceId && b.BaseRefType == baseRefDto.BaseRefType);
                if (oldBaseRef != null)
                    await _sorFileRepository.RemoveSorBytesAsync(oldBaseRef.SorFileId);

                var sorFileId = 0;
                if (baseRefDto.Id != Guid.Empty)
                    sorFileId = await _sorFileRepository.AddSorBytesAsync(baseRefDto.SorBytes!);

                var baseRef = new BaseRef
                {
                    TraceId = dto.TraceId,

                    Id = baseRefDto.Id,
                    BaseRefType = baseRefDto.BaseRefType,
                    SaveTimestamp = DateTime.Now,
                    Duration = baseRefDto.Duration,
                    UserName = baseRefDto.UserName,

                    SorFileId = sorFileId,
                };
                command.BaseRefs.Add(baseRef);
            }
            return command;
        }  
    }
}
