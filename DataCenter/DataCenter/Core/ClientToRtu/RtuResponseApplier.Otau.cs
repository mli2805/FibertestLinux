using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public partial class RtuResponseApplier
{
    public async Task<string> ApplyOtauAttachmentResult(AttachOtauDto dto, string jsonResult)
    {
        var result = Deserialize<OtauAttachedDto>(jsonResult);

        if (result.IsAttached)
        {
            var res = await _responseToEventSourcing.ApplyOtauAttachmentResult(dto, result);
            if (res == null)
                await _ftSignalRClient.NotifyAll("FetchTree", null);
            _logger.Info(Logs.DataCenter, "OTAU attached successfully.");
        }
        else
            _logger.Error(Logs.DataCenter, "Failed to attach OTAU!");

        return jsonResult;
    }

    public async Task<string> ApplyOtauDetachmentResult(DetachOtauDto dto, string jsonResult)
    {
        var result = Deserialize<OtauDetachedDto>(jsonResult);

        if (result.IsDetached)
        {
            var res = await _responseToEventSourcing.ApplyOtauDetachmentResult(dto);
            if (res == null)
                await _ftSignalRClient.NotifyAll("FetchTree", null);
            _logger.Info(Logs.DataCenter, "OTAU detached successfully.");
        }
        else
            _logger.Error(Logs.DataCenter, "Failed to detach OTAU!");

        return jsonResult;
    }
}