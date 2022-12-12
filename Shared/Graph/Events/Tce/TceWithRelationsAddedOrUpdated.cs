namespace Fibertest.Graph;

public class TceWithRelationsAddedOrUpdated
{
    public Guid Id;
    public string Title = "";
    public TceTypeStruct TceTypeStruct;
    public string Ip = "0.0.0.0";
    public List<TceSlot> Slots = new();
    public bool ProcessSnmpTraps;
    public string Comment = "";

    public List<GponPortRelation> AllRelationsOfTce = new();

    public List<Guid> ExcludedTraceIds = new();
}