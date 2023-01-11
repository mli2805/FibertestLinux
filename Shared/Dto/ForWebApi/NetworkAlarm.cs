namespace Fibertest.Dto;

public class NetworkAlarm
{
    public int EventId;
    public Guid RtuId;
    public string? Channel; // Main or Reserve
    public bool HasBeenSeen;
}

public class OpticalAlarm
{
    public int SorFileId;
    public Guid TraceId;
    public bool HasBeenSeen;
}

public class BopAlarm
{
    public int EventId;
    public string? Serial;
    public bool HasBeenSeen;
}

public class AlarmsDto
{
    public List<NetworkAlarm>? NetworkAlarms;
    public List<OpticalAlarm>? OpticalAlarms;
    public List<BopAlarm>? BopAlarms;
}