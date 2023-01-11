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
}