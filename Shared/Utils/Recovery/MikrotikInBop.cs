using Microsoft.Extensions.Logging;

namespace Fibertest.Utils.Recovery;

public static class MikrotikInBop
{
    public static void ConnectAndReboot<T>(ILogger<T> logger, EventId eventId, string ip, int connectionTimeout)
    {
        logger.LLog(eventId, $"Connect Mikrotik {ip} started...");
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
        logger.LLog(eventId, "Connected and logged in successfully.");
        logger.LLog(eventId, $"Reboot Mikrotik {ip} started...");
        mikrotik.Send("/system/reboot", true);
        mikrotik.Read();
        mikrotik.Close();
    }
}