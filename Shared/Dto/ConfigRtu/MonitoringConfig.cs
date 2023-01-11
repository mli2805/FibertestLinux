namespace Fibertest.Dto;

public class MonitoringConfig
{
    public bool IsMonitoringOn { get; set; }

    public int PreciseMakeTimespan { get; set; } = 3600;
    public int PreciseSaveTimespan { get; set; } = 3600;
    public int FastSaveTimespan { get; set; } = 3600;

    public string LastMeasurementTimestamp { get; set; } = string.Empty;
    public bool ShouldSaveSorData { get; set; }
}