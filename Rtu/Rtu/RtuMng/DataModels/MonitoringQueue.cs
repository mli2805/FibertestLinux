using System.Security.Cryptography;
using Fibertest.Dto;
using Fibertest.Utils;
using Fibertest.Utils.Setup;
using Newtonsoft.Json;

namespace Fibertest.Rtu;

public class MonitoringQueue
{
    private readonly ILogger<MonitoringQueue> _logger;

    private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private string _monitoringSettingsFile = null!;
    private string _monitoringSettingFileBackup = null!;
    private string _monitoringSettingsMd5File = null!;
    public Queue<MonitoringPort> Queue = null!;

    public MonitoringQueue(ILogger<MonitoringQueue> logger)
    {
        _logger = logger;
    }

    public int Count() { return Queue.Count; }

    public MonitoringPort Peek() { return Queue.Peek(); }
    public MonitoringPort Dequeue() { return Queue.Dequeue(); }
    public void Enqueue(MonitoringPort item) { Queue.Enqueue(item); }

    public async Task Load()
    {
        _logger.TimestampWithoutMessage(Logs.RtuManager);
        _logger.Info(Logs.RtuManager, "Monitoring queue assembling...");

        var fibertestPath = FileOperations.GetMainFolder();
        var queueFolder = Path.Combine(fibertestPath, @"config");
        _monitoringSettingsFile = Path.Combine(queueFolder, "queue.json");
        _monitoringSettingFileBackup = Path.Combine(queueFolder, "queue.json.bac");
        _monitoringSettingsMd5File = Path.Combine(queueFolder, "queue.json.md5");

        Queue = new Queue<MonitoringPort>();

        if (!File.Exists(_monitoringSettingsFile))
        {
            _logger.Info(Logs.RtuManager, "Queue file not found. Create empty file:");
            _logger.Info(Logs.RtuManager, _monitoringSettingsFile);

            await Save();
        }

        try
        {
            var list = await LoadWithMd5();

            foreach (var port in list)
            {
                Queue.Enqueue(new MonitoringPort(port));
            }
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"Queue parsing: {e.Message}");
        }

        _logger.Info(Logs.RtuManager, $"{Queue.Count} port(s) in queue.");
    }

    private async Task<List<MonitoringPortOnDisk>> LoadWithMd5()
    {
        try
        {
            if (File.Exists(_monitoringSettingsFile))
            {
                if (File.Exists(_monitoringSettingsMd5File))
                {
                    var md5 = CalculateMd5(_monitoringSettingsFile);
                    var md5FromFile = await File.ReadAllTextAsync(_monitoringSettingsMd5File);
                    var content =
                        await File.ReadAllTextAsync(md5 == md5FromFile
                            ? _monitoringSettingsFile
                            : _monitoringSettingFileBackup);

                    var list = JsonConvert.DeserializeObject<List<MonitoringPortOnDisk>>(content);
                    return list ?? new List<MonitoringPortOnDisk>();
                }
            }
            else if (File.Exists(_monitoringSettingFileBackup))
            {
                var contentBackup = await File.ReadAllTextAsync(_monitoringSettingFileBackup);
                return JsonConvert.DeserializeObject<List<MonitoringPortOnDisk>>(contentBackup) 
                       ?? new List<MonitoringPortOnDisk>();
            }
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"Queue loading: {e.Message}");
            if (e.InnerException != null)
            {
                _logger.Error(Logs.RtuManager, $"Queue loading: {e.InnerException.Message}");
            }
        }

        return new List<MonitoringPortOnDisk>();
    }

    public async Task Save()
    {
        try
        {
            var list = Queue.Select(p => new MonitoringPortOnDisk(p)).ToList();
            var content = JsonConvert.SerializeObject(list, JsonSerializerSettings);
            await File.WriteAllTextAsync(_monitoringSettingsFile, content);

            var md5 = CalculateMd5(_monitoringSettingsFile);
            await File.WriteAllTextAsync(_monitoringSettingsMd5File, md5);
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"Queue saving: {e.Message}");
        }
    }

    public async Task SaveBackup()
    {
        try
        {
            var list = Queue.Select(p => new MonitoringPortOnDisk(p)).ToList();
            var content = JsonConvert.SerializeObject(list, JsonSerializerSettings);
            await File.WriteAllTextAsync(_monitoringSettingsFile, content);
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"Queue backup saving: {e.Message}");
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