using System.Security.Cryptography;
using Fibertest.Dto;
using Fibertest.Utils;
using Newtonsoft.Json;

namespace Fibertest.Rtu;

public class MonitoringQueue
{
    private readonly ILogger<MonitoringQueue> _logger;

    private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private readonly string _monitoringSettingsFile = @"..\config\monitoring.que";
    private readonly string _monitoringSettingFileBackup = @"..\config\monitoring.que.bac";
    private readonly string _monitoringSettingsMd5File = @"..\config\monitoring.que.md5";
    public Queue<MonitoringPort> Queue = null!;

    public MonitoringQueue(ILogger<MonitoringQueue> logger)
    {
        _logger = logger;
    }

    public int Count() { return Queue.Count; }

    public MonitoringPort Peek() { return Queue.Peek(); }
    public MonitoringPort Dequeue() { return Queue.Dequeue(); }
    public void Enqueue(MonitoringPort item) { Queue.Enqueue(item); }

    private string[] LoadWithMd5()
    {
        try
        {
            if (File.Exists(_monitoringSettingsFile))
            {
                if (File.Exists(_monitoringSettingsMd5File))
                {
                    var md5 = CalculateMd5(_monitoringSettingsFile);
                    var md5FromFile = File.ReadAllText(_monitoringSettingsMd5File);
                    return File.ReadAllLines(md5 == md5FromFile ? _monitoringSettingsFile : _monitoringSettingFileBackup);
                }
            }
            else if (File.Exists(_monitoringSettingFileBackup))
            {
                return File.ReadAllLines(_monitoringSettingFileBackup);
            }
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Queue loading: {e.Message}");
        }

        return new string[0];
    }

    public void Load()
    {
        _logger.TimestampWithoutMessage(Logs.RtuManager.ToInt());
        _logger.LLog(Logs.RtuManager.ToInt(), "Monitoring queue assembling...");
        Queue = new Queue<MonitoringPort>();

        try
        {

            var contents = LoadWithMd5();
            var list = contents
                .Select(s => (MonitoringPortOnDisk?)JsonConvert.DeserializeObject(s, JsonSerializerSettings))
                .ToList();

            foreach (var port in list)
            {
                if (port != null)
                    Queue.Enqueue(new MonitoringPort(port));
            }
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Queue parsing: {e.Message}");
        }

        _logger.LLog(Logs.RtuManager.ToInt(), $"{Queue.Count} port(s) in queue.");
    }

    public void Save()
    {
        try
        {
            var list = Queue.Select(p => JsonConvert.SerializeObject(new MonitoringPortOnDisk(p), JsonSerializerSettings)).ToList();
            File.WriteAllLines(_monitoringSettingsFile, list);
            var md5 = CalculateMd5(_monitoringSettingsFile);
            File.WriteAllText(_monitoringSettingsMd5File, md5);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Queue saving: {e.Message}");
        }
    }

    public void SaveBackup()
    {
        try
        {
            var list = Queue.Select(p => JsonConvert.SerializeObject(new MonitoringPortOnDisk(p), JsonSerializerSettings)).ToList();
            File.WriteAllLines(_monitoringSettingFileBackup, list);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Queue saving: {e.Message}");
        }
    }

    public void ComposeNewQueue(List<PortWithTraceDto> ports)
    {
        var oldQueue = Queue.Select(p => p).ToList();
        Queue.Clear();

        foreach (var portWithTrace in ports)
        {
            var monitoringPort = new MonitoringPort(portWithTrace);
            var oldPort = oldQueue.FirstOrDefault(p => p.TraceId == monitoringPort.TraceId);
            if (oldPort != null)
            {
                monitoringPort.LastFastSavedTimestamp = oldPort.LastFastSavedTimestamp;
                monitoringPort.LastPreciseSavedTimestamp = oldPort.LastPreciseSavedTimestamp;
            }
            Queue.Enqueue(monitoringPort);
        }
    }

    public void RaiseMonitoringModeChangedFlag()
    {
        var temp = Queue.ToList();
        Queue.Clear();
        foreach (var monitorigPort in temp)
        {
            monitorigPort.IsMonitoringModeChanged = true;
            Queue.Enqueue(monitorigPort);
        }
    }

    static string CalculateMd5(string filename)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filename))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

}