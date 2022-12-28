namespace Fibertest.Dto;

public class EventSourcingDto
{
    public Guid StreamIdOriginal;
    public int SnapshotLastEvent;
    public DateTime SnapshotLastDate;
}