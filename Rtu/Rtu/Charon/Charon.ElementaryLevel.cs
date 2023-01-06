using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class Charon
{
    public bool ResetOtau()
    {
        SendCommand("otau_reset\r\n");
        if (!IsLastCommandSuccessful)
            return false;
        return LastAnswer == "OK\r\n";
    }


    public string GetSerial()
    {
        SendCommand("get_rtu_number\r\n");
        return LastAnswer;
    }

    private int GetOwnPortCount()
    {
        SendCommand("otau_get_count_channels\r\n");
        if (!IsLastCommandSuccessful)
            return -1;

        int ownPortCount;
        if (int.TryParse(LastAnswer, out ownPortCount))
            return ownPortCount;

        LastErrorMessage = "Invalid port count";
        IsLastCommandSuccessful = false;
        return -1;
    }

    public string ShowOnDisplayMessageReady()
    {
        SendCommand("pc_loaded\r\n");
        return !IsLastCommandSuccessful ? LastErrorMessage : "";
    }

    public string ShowMessageMeasurementPort()
    {
        SendCommand("meas\r\n");
        return !IsLastCommandSuccessful ? LastErrorMessage : "";
    }

    private int GetIniSize()
    {
        try
        {
            SendCommand("ini_size\r\n");
            if (!IsLastCommandSuccessful)
                return 0; // read error

            if (LastAnswer.Length >= 15 && LastAnswer.Substring(0, 15) == "ERROR_COMMAND\r\n")
                return 480; // charon too old, know nothing about ini file size

            var lines = LastAnswer.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            CharonIniSize = int.Parse(lines[0]);
            _logger.LLog(Logs.RtuManager.ToInt(), $"Charon ini size is {CharonIniSize}");
            return CharonIniSize;
        }
        catch (Exception e)
        {
            if (IsLastCommandSuccessful)
            {
                IsLastCommandSuccessful = false;
                LastErrorMessage = $"{e.Message} in GetIniSize!";
                _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), LastErrorMessage);
            }

            return 0;
        }
    }

    private Dictionary<int, NetAddress>? GetExtendedPorts()
    {
        try
        {
            ReadIniFile();
            if (!IsLastCommandSuccessful)
            {
                // read iniFile error
                LastErrorMessage = $"Get extended ports error {LastErrorMessage}";
                _logger.LLog(Logs.RtuManager.ToInt(), LastErrorMessage);
                return null;
            }

            if (LastAnswer.Substring(0, 15) == "ERROR_COMMAND\r\n")
            {
                // charon too old, knows nothing about extensions
                _logger.Log(LogLevel.Warning, Logs.RtuManager.ToInt(), "Charon too old, knows nothing about extensions");
                return new Dictionary<int, NetAddress>();
            }

            if (LastAnswer.Substring(0, 22) == "[OpticalPortExtension]")
                return ParseIniContent(LastAnswer);

            return new Dictionary<int, NetAddress>();
        }
        catch (Exception e)
        {
            if (IsLastCommandSuccessful)
            {
                IsLastCommandSuccessful = false;
                LastErrorMessage = $"{e.Message} in GetExtendedPorts!";
            }
            return null;
        }
    }

    private void ReadIniFile() { SendCommand("ini_read\r\n"); }

    private Dictionary<int, NetAddress> ParseIniContent(string content)
    {
        var result = new Dictionary<int, NetAddress>();
        string[] separator = new[] { "\r\n" };
        var lines = content.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length - 1; i++)
        {
            var parts = lines[i].Split('=');
            var addressParts = parts[1].Split(':');
            int port;
            int.TryParse(addressParts[1], out port);
            result.Add(int.Parse(parts[0]), new NetAddress(addressParts[0], port));
        }
        return result;
    }

    private int SetActivePort(int port)
    {
        if (port < 1 || port > OwnPortCount)
        {
            LastErrorMessage = $"Port number should be from 1 to {OwnPortCount}";
            IsLastCommandSuccessful = false;
            return -1;
        }

        SendCommand($"otau_set_channel {port} d\r\n");
        if (!IsLastCommandSuccessful)
            return -1;
        var resultingPort = GetActivePort();
        if (!IsLastCommandSuccessful)
            return -1;
        if (resultingPort != port)
            LastErrorMessage = "Set active port number error";
        return resultingPort;
    }

    private int GetActivePort()
    {
        SendCommand("otau_get_channel\r\n");
        if (!IsLastCommandSuccessful)
            return -1;

        int activePort;
        if (int.TryParse(LastAnswer, out activePort))
            return activePort;

        IsLastCommandSuccessful = false;
        return -1;
    }

    private string DictionaryToContent(Dictionary<int, NetAddress> extPorts)
    {
        if (extPorts.Count == 0)
            return Environment.NewLine;
        var result = "[OpticalPortExtension]" + Environment.NewLine;
        foreach (var extPort in extPorts)
            result += $"{extPort.Key}={extPort.Value.ToStringA()}" + Environment.NewLine;
        return result;
    }

}