using Fibertest.Dto;

namespace Graph
{
    
    public class Node
    {
        public Guid NodeId;
        public string? Title;
        public EquipmentType TypeOfLastAddedEquipment;
        public FiberState State;
        public PointLatLng Position;
        public string? Comment;

        public Guid AccidentOnTraceId;

        public bool IsHighlighted
        {
            get; 
            set;
        }
    }
}