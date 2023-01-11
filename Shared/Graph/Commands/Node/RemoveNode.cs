using Fibertest.Dto;

namespace Fibertest.Graph;

public class NodeDetour
{
    public Guid FiberId;
    public Guid NodeId1;
    public Guid NodeId2;
    public FiberState TraceState;
    public Guid TraceId;
}

public class RemoveNode
{
    public Guid NodeId;
    public bool IsAdjustmentPoint;

    public List<NodeDetour> DetoursForGraph = new List<NodeDetour>();
    public Guid FiberIdToDetourAdjustmentPoint; // if there are no traces passing through this point
}