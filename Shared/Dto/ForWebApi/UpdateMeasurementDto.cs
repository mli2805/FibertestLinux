namespace Fibertest.Dto
{
    public class UpdateMeasurementDto
    {
        public string? ClientIp;
        public int SorFileId;

        public EventStatus EventStatus;
        public DateTime StatusChangedTimestamp;
        public string? StatusChangedByUser;

        public string? Comment;
    }
}