// ReSharper disable InconsistentNaming

namespace Fibertest.Dto;

// Root myDeserializedClass = JsonConvert.DeserializeObject<VeexNotification>(myJsonResponse); 
public class Change
{
    public double changeLocation;
    public string? changeType;
    public int currentEventIndex;
    public double currentEventLeadingLossCoefficient;
    public double currentEventReflectance;
    public string? currentEventType;
    public int referenceEventIndex;
    public bool referenceEventMapsToCurrentEvent;
    public string? referenceEventType;
}

public class TraceChange
{
    public double changeLocation;
    public string? changeType;
    public List<Change>? changes;
    public int currentEventIndex;
    public double currentEventLeadingLossCoefficient;
    public double currentEventReflectance;
    public string? currentEventType;
    public string? levelName;
    public List<VeexMeasurementLevel>? levels;
    public int referenceEventIndex;
    public bool referenceEventMapsToCurrentEvent;
    public string? referenceEventType;
}

public class VeexMeasurementLevel
{
    public List<Change>? changes;
    public string? levelName;
}

public class Data
{
    public string? result;
    public List<VeexOtauPort>? OtauPorts;
    public DateTime started;
    public string? testId;
    public string? testName;
    public string? type;
    public string? extendedResult;
    public TraceChange? traceChange;
}

public class VeexNotificationEvent
{
    public Data? data;
    public DateTime time;
    public string? type;
}

public class VeexNotification
{
    public List<VeexNotificationEvent>? events;
    public string? type;
}