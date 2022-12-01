namespace Fibertest.Dto
{
    public class TraceInformationDto
    {
        public TraceHeaderDto Header = new TraceHeaderDto();

        public List<TraceInfoTableItem>? Equipment;
        public List<TraceInfoTableItem>? Nodes;

        public bool IsLightMonitoring;
        public string? Comment;
    }
}