namespace Fibertest.Graph;

public class Zone
{
    public Guid ZoneId;
    public bool IsDefaultZone;

    public string Title;
    public string? Comment;

    public Zone(string title)
    {
        Title = title;
    }

    public override string ToString()
    {
        return Title;
    }
}