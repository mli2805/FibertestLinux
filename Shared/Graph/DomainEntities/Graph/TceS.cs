namespace Fibertest.Graph
{
    
    public class TceS
    {
        public Guid Id = Guid.NewGuid();
        public string? Title;
        public TceTypeStruct TceTypeStruct;
        public string? Ip = @"0.0.0.0";
        public List<TceSlot> Slots = new();
        public bool ProcessSnmpTraps;
        public string? Comment;

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
}