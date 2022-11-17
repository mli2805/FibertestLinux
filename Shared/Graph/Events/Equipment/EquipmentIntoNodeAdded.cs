using Fibertest.Dto;

namespace Graph
{
    
    public class EquipmentIntoNodeAdded
    {
        public Guid EquipmentId;
        public Guid NodeId;
        public string? Title;
        public EquipmentType Type;
        public int CableReserveLeft;
        public int CableReserveRight;
        public string? Comment;

        public List<Guid> TracesForInsertion = new();
    }
}
