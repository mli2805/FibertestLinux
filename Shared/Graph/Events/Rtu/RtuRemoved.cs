namespace Graph
{
    
    public class RtuRemoved
    {
        public Guid RtuId;
        public Guid RtuNodeId;

        // fiberId - traceId 
        // don't use Dictionary  because fiber could conduct more than one trace
        public List<KeyValuePair<Guid, Guid>>? FibersFromCleanedTraces;
    }
}
