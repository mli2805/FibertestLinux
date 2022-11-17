using Fibertest.Dto;

namespace Graph
{
    
    public class EquipmentUpdated
    {
        public Guid EquipmentId;
        public string? Title;
        public EquipmentType Type;
        public int CableReserveLeft;
        public int CableReserveRight;
        public string? Comment;
    }
}