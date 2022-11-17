using Fibertest.Dto;

namespace Fibertest.Graph
{
    public class UpdateEquipment
    {
        public Guid EquipmentId;
        public string? Title;
        public EquipmentType Type;
        public int CableReserveLeft;
        public int CableReserveRight;
        public string? Comment;
    }
}