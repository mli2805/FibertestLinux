using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Fibertest.Utils.Recovery;

public static class RestoreFunctions
{
    public static void ClearArp(ILogger logger)
    {
        var res = Arp.GetTable();
        logger.LogInfo(Logs.RtuService, res ?? "");
        Arp.ClearCache();
        logger.LogInfo(Logs.RtuManager, "Recovery procedure: Clear ARP table.");
        logger.LogInfo(Logs.RtuManager, "Recovery procedure: Reset Charon");
        logger.LogInfo(Logs.RtuService, "Recovery procedure: Clear ARP table and Reset Charon.");
        res = Arp.GetTable();
        logger.LogInfo(Logs.RtuService, res ?? "");
    }

    public static void RebootSystem(ILogger logger, int delay)
    {
        logger.LogInfo(Logs.RtuManager, $"Recovery procedure: System reboot in {delay} sec...");
        ProcessStartInfo proc = new ProcessStartInfo
        {
            FileName = "cmd",
            WindowStyle = ProcessWindowStyle.Hidden,
            Arguments = $"/C shutdown -f -r -t {delay}"
        };

        try
        {
            Process.Start(proc);
        }
        catch (Exception e)
        {
            logger.LogError(Logs.RtuManager, e.Message);
        }
    }
}