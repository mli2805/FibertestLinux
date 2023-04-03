namespace Fibertest.Rtu
{
    public static class CancellationExt
    {
        public static bool IsCancellationRequested(this CancellationToken[] tokens)
        {
            return tokens.Any(t => t.IsCancellationRequested);
        }
    }
}
