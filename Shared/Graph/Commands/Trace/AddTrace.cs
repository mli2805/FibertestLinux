namespace Fibertest.Graph
{
    public class AddTrace
    {
        public Guid TraceId;
        public Guid RtuId;
        public string? Title;
        public List<Guid> NodeIds = new();
        public List<Guid> EquipmentIds = new();
        public List<Guid> FiberIds = new();
        public string? Comment;
    }
}
