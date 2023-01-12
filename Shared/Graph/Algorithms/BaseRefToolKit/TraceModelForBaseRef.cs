namespace Fibertest.Graph;

public class TraceModelForBaseRef
{
    public Node[] NodeArray;
    public Equipment[] EquipArray;
    public Fiber[]? FiberArray; // intermediary data - just for calculate distances
    public int[]? DistancesMm;

    public TraceModelForBaseRef(Node[] nodeArray, Equipment[] equipArray, Fiber[] fiberArray)
    {
        NodeArray = nodeArray;
        EquipArray = equipArray;
        FiberArray = fiberArray;
    } 
    
    public TraceModelForBaseRef(Node[] nodeArray, Equipment[] equipArray, int[] distancesArray)
    {
        NodeArray = nodeArray;
        EquipArray = equipArray;
        DistancesMm = distancesArray;
    }
}