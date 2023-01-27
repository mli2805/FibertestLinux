namespace Fibertest.Dto;

public class TraceInfoTableItem
{
    public string? NodeType { get; set; }
    public int Count { get; set; }

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