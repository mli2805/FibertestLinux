using Fibertest.Dto;

namespace Fibertest.Graph
{
    public class AddEquipmentAtGpsLocation
    {
        public Guid EmptyNodeEquipmentId; 
        public Guid RequestedEquipmentId; 
        public Guid NodeId;
        public EquipmentType Type;
        public double Latitude;
        public double Longitude;

    }
}
