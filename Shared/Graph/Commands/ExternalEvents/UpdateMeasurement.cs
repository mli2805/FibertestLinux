using Fibertest.Dto;

namespace Graph
{
    public class UpdateMeasurement
    {
        public int SorFileId;

        public EventStatus EventStatus;
        public DateTime StatusChangedTimestamp;
        public string? StatusChangedByUser;

        public string? Comment;
    }
}