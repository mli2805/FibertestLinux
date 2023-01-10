using System.Diagnostics;

namespace Fibertest.Utils.Recovery;

public static class Arp
{
    public static string? GetTable()
    {
        Process? p = null;
        string? output;
        try
        {
            p = Process.Start(new ProcessStartInfo("arp", "-a")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            output = p?.StandardOutput.ReadToEnd();
        }
        catch (Exception ex)
        {
            return $"IPInfo: Error Retrieving 'arp -a' Results {ex.Message}";
        }
        finally
        {
            p?.Close();
        }

        return output;
    }

    public static string? ClearCache()
    {
        Process? p = null;
        string? output;
        try
        {
            p = Process.Start(new ProcessStartInfo("netsh", "interface ip delete arpcache")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            output = p?.StandardOutput.ReadToEnd();
        }
        catch (Exception ex)
        {
            return $"IPInfo: Error Retrieving 'netsh interface ip delete arpcache' Results {ex.Message}";
        }
        finally
        {
            p?.Close();
        }

        return output;
    }
}