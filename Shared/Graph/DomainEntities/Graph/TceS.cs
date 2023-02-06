namespace Fibertest.Graph;

[Serializable]
public class TceS
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public TceTypeStruct TceTypeStruct { get; set; }
    public string Ip { get; set; } = @"0.0.0.0";
    public List<TceSlot> Slots { get; set; } = new();
    public bool ProcessSnmpTraps { get; set; }
    public string Comment { get; set; } = "";

    public TceS()
    {
    }

    public TceS(TceS source)
    {
        Id = source.Id;
        Title = source.Title;
        TceTypeStruct = source.TceTypeStruct;
        Ip = source.Ip;
        Slots = new List<TceSlot>();
        foreach (var sourceSlot in source.Slots)
        {
            Slots.Add(new TceSlot()
            {
                Position = sourceSlot.Position,
                GponInterfaceCount = sourceSlot.GponInterfaceCount,
            });
        }

        ProcessSnmpTraps = source.ProcessSnmpTraps;
        Comment = source.Comment;
    }
}