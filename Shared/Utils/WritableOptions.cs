// https://learn.microsoft.com/en-us/answers/questions/609232/how-to-save-the-updates-i-made-to-appsettings-conf.html?childToView=1092152#comment-1092152


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fibertest.Utils;

public interface IWritableOptions<out T> : IOptions<T> where T : class, new()
{
    void Update(Action<T> applyChanges);
}
public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
{
    private readonly IOptionsMonitor<T> _options;
    private readonly IConfigurationRoot _configuration;
    private readonly string _section;
    private readonly string _file;
    public WritableOptions(
        IOptionsMonitor<T> options,
        IConfigurationRoot configuration,
        string section,
        string file)
    {
        _options = options;
        _configuration = configuration;
        _section = section;
        _file = file;
    }
    public T Value => _options.CurrentValue;
    public T Get(string name) => _options.Get(name);
    public void Update(Action<T> applyChanges)
    {
        // var fileProvider = _environment.ContentRootFileProvider;
        // var fileInfo = fileProvider.GetFileInfo(_file);
        // var physicalPath = fileInfo.PhysicalPath;
        // var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(physicalPath));
        var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(_file));
        if (jObject == null) return;
        var sectionObject = jObject.TryGetValue(_section, out JToken? section) 
            ? JsonConvert.DeserializeObject<T>(section.ToString()) ?? Value
            : Value;
        applyChanges(sectionObject);
        jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
        // File.WriteAllText(physicalPath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
        File.WriteAllText(_file, JsonConvert.SerializeObject(jObject, Formatting.Indented));
        _configuration.Reload();
    }
}