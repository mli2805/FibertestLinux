namespace Fibertest.Graph;

public class AddRtuAtGpsLocation
{
    public Guid Id;
    public Guid NodeId;
    public double Latitude;
    public double Longitude;
    public string Title;
    public string Comment = "";

    public AddRtuAtGpsLocation(Guid id, Guid nodeId, double latitude, double longitude, string title)
    {
        Id = id;
        NodeId = nodeId;
        Latitude = latitude;
        Longitude = longitude;
        Title = title;
    }
}