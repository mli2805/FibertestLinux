namespace Fibertest.Dto
{
    public class TraceInfoTableItem
    {
        public string? NodeType;
        public int Count;

        // WCF data transfer needs parameterless constructor
        public TraceInfoTableItem()
        {
        }

        public TraceInfoTableItem(string nodeType, int count)
        {
            NodeType = nodeType;
            Count = count;
        }
    }
}