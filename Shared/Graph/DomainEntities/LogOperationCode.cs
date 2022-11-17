namespace Graph
{
    public enum LogOperationCode
    {
        ClientStarted = 101,
        ClientExited = 102,
        ClientConnectionLost = 103,
        UsersMachineKeyAssigned,

        RtuAdded = 201,
        RtuUpdated,
        RtuInitialized,
        RtuRemoved,

        TraceAdded = 301,
        TraceUpdated,
        TraceAttached,
        TraceDetached,
        TraceCleaned,
        TraceRemoved,

        TceAdded,
        TceUpdated,
        TceRemoved,

        BaseRefAssigned = 401,
        MonitoringSettingsChanged,
        MonitoringStarted,
        MonitoringStopped,

        MeasurementUpdated,

        EventsAndSorsRemoved,
        SnapshotMade,
    }
}