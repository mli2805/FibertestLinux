namespace Fibertest.Dto;

public class MonitoringConfig
{
    public bool IsMonitoringOnPersisted { get; set; }
    public bool KeepOtdrConnection { get; set; }
    public bool IsAutoBaseMeasurementInProgress { get; set; }

    public int PreciseMakeTimespan { get; set; } = 3600;
    public int PreciseSaveTimespan { get; set; } = 3600;
    public int FastSaveTimespan { get; set; } = 3600;

    public string LastMeasurementTimestamp { get; set; } = string.Empty;
    public bool ShouldSaveSorData { get; set; }
}