using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class RtuAutoBaseProgress
    {
        public readonly TraceLeaf TraceLeaf;
        public readonly Trace Trace;
        public readonly int Ordinal;
        public bool MeasurementDone;
        public bool BaseRefAssigned;

        public RtuAutoBaseProgress(TraceLeaf traceLeaf, Trace trace, int ordinal)
        {
            TraceLeaf = traceLeaf;
            Trace = trace;
            Ordinal = ordinal;
        }
    }
}