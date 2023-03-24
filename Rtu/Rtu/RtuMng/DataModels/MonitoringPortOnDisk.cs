using Fibertest.Dto;

namespace Fibertest.Rtu;

public class MonitoringPortOnDisk
{
    public string Serial = null!;
    public int OpticalPort;
    public bool IsPortOnMainCharon;
    public Guid TraceId;
    public FiberState LastTraceState;
    public MoniResult? LastMoniResult;

    public DateTime? LastPreciseMadeTimestamp;
    public DateTime LastPreciseSavedTimestamp;
    public DateTime LastFastSavedTimestamp;

    public bool IsMonitoringModeChanged;
    public bool IsConfirmationRequired;
       
    // for deserializer
    public MonitoringPortOnDisk()
    {
        
    }

    public MonitoringPortOnDisk(MonitoringPort port)
    {
        Serial = port.CharonSerial;
        OpticalPort = port.OpticalPort;
        IsPortOnMainCharon = port.IsPortOnMainCharon;
        TraceId = port.TraceId;
        LastTraceState = port.LastTraceState;

        if (port.LastMoniResult != null)
            LastMoniResult = new MoniResult()
            {
                IsNoFiber = port.LastMoniResult.IsNoFiber,
                IsFiberBreak = port.LastMoniResult.IsFiberBreak,
                Levels = port.LastMoniResult.Levels,
                BaseRefType = port.LastMoniResult.BaseRefType,
                FirstBreakDistance = port.LastMoniResult.FirstBreakDistance,
                Accidents = port.LastMoniResult.Accidents,
            };

        LastPreciseMadeTimestamp = port.LastPreciseMadeTimestamp;
        LastFastSavedTimestamp = port.LastFastSavedTimestamp;
        LastPreciseSavedTimestamp = port.LastPreciseSavedTimestamp;

        IsMonitoringModeChanged = port.IsMonitoringModeChanged;
        IsConfirmationRequired = port.IsConfirmationRequired;
    }
}