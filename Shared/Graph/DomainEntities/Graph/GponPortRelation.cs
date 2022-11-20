using Fibertest.Dto;

namespace Fibertest.Graph
{
    
    public class GponPortRelation
    {
        // public Guid Id;
        public Guid TceId;
        public int SlotPosition;
        public int GponInterface;
        public Guid RtuId;
        public OtauPortDto? OtauPortDto;
        public Guid TraceId;
    }
}