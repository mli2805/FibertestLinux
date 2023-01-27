using Fibertest.CharonLib;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public class MonitoringPort
{
    public bool IsPortOnMainCharon;

    public string CharonSerial;
    public int OpticalPort;
    public Guid TraceId;

    public DateTime? LastPreciseMadeTimestamp;
    public DateTime LastPreciseSavedTimestamp;
    public DateTime LastFastSavedTimestamp;

    public FiberState LastTraceState;
    public MoniResult? LastMoniResult;
    public bool IsBreakdownCloserThen20Km;

    public bool IsMonitoringModeChanged;
    public bool IsConfirmationRequired;

    public MonitoringPort(MonitoringPortOnDisk port)
    {
        CharonSerial = port.Serial;
        OpticalPort = port.OpticalPort;
        TraceId = port.TraceId;
        IsPortOnMainCharon = port.IsPortOnMainCharon;
        LastTraceState = port.LastTraceState;

        LastPreciseMadeTimestamp = port.LastPreciseMadeTimestamp;
        LastFastSavedTimestamp = port.LastFastSavedTimestamp;
        LastPreciseSavedTimestamp = port.LastPreciseSavedTimestamp;

        IsMonitoringModeChanged = port.IsMonitoringModeChanged;
        IsConfirmationRequired = port.IsConfirmationRequired;

        if (port.LastMoniResult != null)
            LastMoniResult = new MoniResult()
            {
                IsNoFiber = port.LastMoniResult.IsNoFiber,
                IsFiberBreak = port.LastMoniResult.IsFiberBreak,
                Levels = port.LastMoniResult.Levels,
                BaseRefType = port.LastMoniResult.BaseRefType,
                FirstBreakDistance = port.LastMoniResult.FirstBreakDistance,
                Accidents = port.LastMoniResult.Accidents
            };
    }

    // new port for monitoring in user's command
    public MonitoringPort(PortWithTraceDto port)
    {
        CharonSerial = port.OtauPort.Serial;
        OpticalPort = port.OtauPort.OpticalPort;
        IsPortOnMainCharon = port.OtauPort.IsPortOnMainCharon;
        TraceId = port.TraceId;
        LastTraceState = port.LastTraceState;

        LastFastSavedTimestamp = DateTime.Now;
        LastPreciseSavedTimestamp = DateTime.Now;

        IsMonitoringModeChanged = true;
    }

    private string GetPortFolderName()
    {
        return $"{CharonSerial}p{OpticalPort:000}";
    }

    private string ToStringA()
    {
        return IsPortOnMainCharon
            ? $"{OpticalPort}"
            : $"{OpticalPort} on {CharonSerial}";
    }

    public string ToStringB(Charon mainCharon)
    {
        if (CharonSerial == mainCharon.Serial)
            return OpticalPort.ToString();
        foreach (var pair in mainCharon.Children)
        {
            if (pair.Value.Serial == CharonSerial)
                return $"{pair.Key}:{OpticalPort}";
        }
        return $"Can't find port {ToStringA()}";
    }

    public bool HasAdditionalBase()
    {
        var basefile = AppDomain.CurrentDomain.BaseDirectory + $@"..\PortData\{GetPortFolderName()}\{BaseRefType.Additional.ToBaseFileName()}";
        return File.Exists(basefile);
    }

    public byte[]? GetBaseBytes<T>(BaseRefType baseRefType, ILogger<T> logger)
    {
        var basefile = AppDomain.CurrentDomain.BaseDirectory + $@"..\PortData\{GetPortFolderName()}\{baseRefType.ToBaseFileName()}";
        if (File.Exists(basefile))
            return File.ReadAllBytes(basefile);
        logger.LogError(Logs.RtuManager, $"Can't find {basefile}");
        return null;
    }

    public void SaveSorData<T>(BaseRefType baseRefType, byte[] bytes, SorType sorType, ILogger<T> logger)
    {
        var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Measurements\");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        var filename = Path.Combine(folder, $@"{DateTime.Now:ddMM HHmmss} {baseRefType} {sorType}.sor");

        try
        {
            File.WriteAllBytes(filename, bytes);
        }
        catch (Exception e)
        {
            logger.LogError(Logs.RtuManager, $"Failed to persist measurement data into {filename}");
            logger.LogError(Logs.RtuManager, e.Message);
        }
    }

    public void SaveMeasBytes<T>(BaseRefType baseRefType, byte[] bytes, SorType sorType, ILogger<T> logger)
    {
        var measfile = AppDomain.CurrentDomain.BaseDirectory + 
                       $@"..\PortData\{GetPortFolderName()}\{baseRefType.ToFileName(sorType)}";

        try
        {
            if (baseRefType == BaseRefType.Precise && sorType == SorType.Meas && File.Exists(measfile))
            {
                var previousFile = AppDomain.CurrentDomain.BaseDirectory
                                   + $@"..\PortData\{GetPortFolderName()}\{baseRefType.ToFileName(SorType.Previous)}";
                if (File.Exists(previousFile))
                    File.Delete(previousFile);
                File.Move(measfile, previousFile);
            }
            File.WriteAllBytes(measfile, bytes);
        }
        catch (Exception e)
        {
            logger.LogError(Logs.RtuManager, $"Failed to persist measurement data into {measfile}");
            logger.LogError(Logs.RtuManager, e.Message);
        }
    }
}