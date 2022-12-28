namespace Fibertest.DataCenter;

public class EventSourcingConfig
{
    public int EventSourcingPortion { get; set; }
    public Guid StreamIdOriginal { get; set; }
    public int SnapshotLastEvent { get; set; }
    public DateTime SnapshotLastDate { get; set; }
}