namespace Fibertest.DataCenter
{
    public class Snapshot
    {
        public int Id;
        public Guid StreamIdOriginal;
        public int LastEventNumber;
        public DateTime LastEventDate;
        public byte[] Payload = Array.Empty<byte>();


    }
}
