using Fibertest.Dto;

namespace Graph
{
    public class EquipmentAtGpsLocationWithNodeTitleAdded
    {
        public Guid EmptyNodeEquipmentId;
        public Guid RequestedEquipmentId;
        public Guid NodeId;
        public EquipmentType Type;
        public double Latitude;
        public double Longitude;

        public string? Title;
        public string? Comment;
    }
}