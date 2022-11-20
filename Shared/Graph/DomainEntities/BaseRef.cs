using Fibertest.Dto;

namespace Fibertest.Graph
{
    
    public class BaseRef
    {
        public Guid Id;

        public Guid TraceId;
        public BaseRefType BaseRefType;
        public string? UserName;
        public DateTime SaveTimestamp;
        public TimeSpan Duration;

        public int SorFileId;
    }
}
