using Microsoft.Extensions.Logging;

namespace Fibertest.Utils.Recovery;

public static class MikrotikInBop
{
    public static void ConnectAndReboot<T>(ILogger<T> logger, EventId eventId, string ip, int connectionTimeout)
    {
        logger.Log(LogLevel.Information, eventId, $"Connect Mikrotik {ip} started...");
        var mikrotik = new Mikrotik(ip, connectionTimeout);
        if (!mikrotik.IsAvailable)
        {
            logger.Log(LogLevel.Error, eventId, $"Couldn't establish tcp connection with Mikrotik {ip}:8728");
            return;
        }
        if (!mikrotik.Login("admin", ""))
        {
            logger.Log(LogLevel.Error, eventId, $"Could not log in Mikrotik {ip}");
            mikrotik.Close();
            return;
        }

        logger.Log(LogLevel.Information, "Connected and logged in successfully.");
        logger.Log(LogLevel.Information, $"Reboot Mikrotik {ip} started...");
        mikrotik.Send("/system/reboot", true);
        mikrotik.Read();
        mikrotik.Close();
    }
}