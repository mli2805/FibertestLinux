using System.Reflection;
using Newtonsoft.Json;

namespace Fibertest.Utils;

public static class ConfigUtils
{
    public static void Validate<T>(string filename, T empty)
    {
        if (!File.Exists(filename))
            File.WriteAllText(filename, JsonConvert.SerializeObject(empty));

        // ���� �� ������� ������-���� ���� -
        //  ��� �������������� ������� �� ��������� �� ��������� ������������ � ������ ������� � ������� � ����
        //  ���� ������ ������� �� ������ ���� nullable
        // ���� ���� �����-�� ���������� ���� -
        //  ��� ����� ��������������� ��� �������������� � �� ������� � ������������ ����
        var json = File.ReadAllText(filename);
        var config = JsonConvert.DeserializeObject<T>(json);
        if (config == null) return;
        File.WriteAllText(filename, JsonConvert.SerializeObject(config));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename">configFile.json</param>
    /// <returns>../config/configFile.json</returns>
    public static string GetConfigPath(string filename)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var basePath = Path.GetDirectoryName(assemblyLocation) ?? "";
        return Path.Combine(basePath, $@"../config/{filename}");
    }
}