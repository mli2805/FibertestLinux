using Fibertest.Dto;

namespace Fibertest.Graph;

public class NodeRemoved
{
    public Guid NodeId;
    public EquipmentType Type;

    public List<NodeDetour> DetoursForGraph = new List<NodeDetour>(); // mapper copies dictionary and list successfully
    public Guid FiberIdToDetourAdjustmentPoint; // if there are no traces passing through this point
}