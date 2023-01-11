namespace Fibertest.Dto;

public class EventSourcingConfig
{
    public int EventSourcingPortion { get; set; } = 100;
    public Guid StreamIdOriginal { get; set; }
    public int SnapshotLastEvent { get; set; }
    public DateTime SnapshotLastDate { get; set; }
}