using Fibertest.Dto;

namespace Fibertest.Graph
{
    public class MeasurementUpdated
    {
        public int SorFileId;

        public EventStatus EventStatus;
        public DateTime StatusChangedTimestamp;
        public string? StatusChangedByUser;

        public string? Comment;
    }
}