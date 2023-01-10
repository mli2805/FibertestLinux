namespace Fibertest.DataCenter;

public class Snapshot
{
    public int Id { get; set; }
    public Guid StreamIdOriginal { get; set; }
    public int LastEventNumber { get; set; }
    public DateTime LastEventDate { get; set; }
    public byte[] Payload { get; set; } = Array.Empty<byte>();
}