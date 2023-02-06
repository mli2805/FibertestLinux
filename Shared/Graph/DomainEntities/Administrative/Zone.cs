namespace Fibertest.Graph;

[Serializable]
public class Zone
{
    public Guid ZoneId;
    public bool IsDefaultZone;

    public string Title { get; set; }
    public string? Comment { get; set; }

    public Zone(string title)
    {
        Title = title;
    }

    public override string ToString()
    {
        return Title;
    }
}