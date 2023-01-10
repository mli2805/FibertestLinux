namespace Fibertest.Graph;

public class AddTrace
{
    public Guid TraceId;
    public Guid RtuId;
    public string? Title;
    public List<Guid> NodeIds;
    public List<Guid> EquipmentIds;
    public List<Guid> FiberIds;
    public string? Comment;

    public AddTrace(Guid traceId, Guid rtuId, List<Guid> nodeIds, List<Guid> equipmentIds, List<Guid> fiberIds)
    {
        TraceId = traceId;
        RtuId = rtuId;
        NodeIds = nodeIds;
        EquipmentIds = equipmentIds;
        FiberIds = fiberIds;
    }
}