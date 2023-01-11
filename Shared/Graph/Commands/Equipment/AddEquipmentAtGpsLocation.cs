using Fibertest.Dto;

namespace Fibertest.Graph;

public class AddEquipmentAtGpsLocation
{
    public Guid EmptyNodeEquipmentId; 
    public Guid RequestedEquipmentId; 
    public Guid NodeId;
    public EquipmentType Type;
    public double Latitude;
    public double Longitude;

    public AddEquipmentAtGpsLocation(EquipmentType type, double latitude, double longitude)
    {
        NodeId = Guid.NewGuid();
        RequestedEquipmentId = Guid.NewGuid();
        EmptyNodeEquipmentId = type == EquipmentType.EmptyNode || type == EquipmentType.AdjustmentPoint 
            ? Guid.Empty : Guid.NewGuid();

        Type = type;
        Latitude = latitude;
        Longitude = longitude;
    }
}