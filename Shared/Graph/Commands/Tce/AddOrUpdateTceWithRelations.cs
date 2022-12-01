namespace Fibertest.Graph;

public class AddOrUpdateTceWithRelations
{
    public Guid Id;
    public string? Title;
    public TceTypeStruct TceTypeStruct;
    public string? Ip;
    public List<TceSlot> Slots = new();
    public bool ProcessSnmpTraps;
    public string? Comment;

    public List<GponPortRelation> AllRelationsOfTce = new();
}