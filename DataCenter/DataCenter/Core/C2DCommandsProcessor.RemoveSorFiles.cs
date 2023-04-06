namespace Fibertest.DataCenter
{
    public partial class C2DCommandsProcessor
    {
        private async Task<string?> RemoveSorFiles(Guid traceId)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return $@"Trace {traceId} not found";

            // starting in another thread breaks tests
            // await Task.Factory.StartNew(() => LongPart(traceId, cleanTrace, username, clientIp));

            return await LongPart(traceId);
        }

        private async Task<string?> LongPart(Guid traceId)
        {
            var sorFileIds = _writeModel.Measurements
                .Where(m => m.TraceId == traceId)
                .Select(l => l.SorFileId).ToArray();
            var sorFileIds2 = sorFileIds.Concat(_writeModel.BaseRefs
                    .Where(b => b.TraceId == traceId)
                    .Select(l => l.SorFileId).ToArray())
                .ToArray();
            await _sorFileRepository.RemoveManySorAsync(sorFileIds2);
            return null;
        }

    }
}
