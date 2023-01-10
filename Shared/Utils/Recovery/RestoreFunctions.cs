using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Fibertest.Utils.Recovery;

public static class RestoreFunctions
{
    public static void ClearArp(ILogger logger)
    {
        var res = Arp.GetTable();
        logger.LLog(Logs.RtuService.ToInt(), res ?? "");
        Arp.ClearCache();
        logger.LLog(Logs.RtuManager.ToInt(), "Recovery procedure: Clear ARP table.");
        logger.LLog(Logs.RtuManager.ToInt(), "Recovery procedure: Reset Charon");
        logger.LLog(Logs.RtuService.ToInt(), "Recovery procedure: Clear ARP table and Reset Charon.");
        res = Arp.GetTable();
        logger.LLog(Logs.RtuService.ToInt(), res ?? "");
    }

    public static void RebootSystem(ILogger logger, int delay)
    {
        logger.LLog(Logs.RtuManager.ToInt(), $"Recovery procedure: System reboot in {delay} sec...");
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
            logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), e.Message);
        }
    }
}