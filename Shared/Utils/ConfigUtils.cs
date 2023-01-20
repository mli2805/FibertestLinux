using Newtonsoft.Json;

namespace Fibertest.Utils;

public static class ConfigUtils
{
    public static void EnsureCreation<T>(string filename) where T : new()
    {
        var empty = new T();

        if (!File.Exists(filename))
        {
            var directoryName = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName!);
            File.WriteAllText(filename, JsonConvert.SerializeObject(empty));
        }

        // если не хватает какого-либо поля -
        //  при десериализации создаст со значением по умолчанию определенным в классе конфига и запишет в файл
        //  поля конфиг классов не должны быть nullable
        // если есть какие-то устаревшие поля -
        //  они будут проигнорированы при десериализации и не попадут в записываемый файл
        var json = File.ReadAllText(filename);
        var config = JsonConvert.DeserializeObject<T>(json);
        if (config == null) return;
        File.WriteAllText(filename, JsonConvert.SerializeObject(config));
    }
}