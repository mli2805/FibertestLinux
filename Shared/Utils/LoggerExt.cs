using Microsoft.Extensions.Logging;

namespace Fibertest.Utils;

public static class LoggerExt
{
    // timestamp without message AND then absolutely empty space
    public static void TimestampWithoutMessage(this ILogger logger, Logs log)
    {
        logger.Log(LogLevel.Information, log.ToInt(), "");
    }

    public static void HyphenLine(this ILogger logger, Logs log)
    {
        logger.Log(LogLevel.Information, log.ToInt(), new string('-', 78));
    }

    public static void StartLine(this ILogger logger, Logs log)
    {
        logger.Log(LogLevel.Information, log.ToInt(), Environment.NewLine + Environment.NewLine + new string('-', 78));
    }

    public static void EmptyAndLog(this ILogger logger, Logs log, string message = "")
    {
        logger.Log(LogLevel.Information, log.ToInt(), "");
        logger.Log(LogLevel.Information, log.ToInt(), message);
    }

    public static void Info(this ILogger logger, Logs log, string message = "")
    {
        logger.Log(LogLevel.Information, log.ToInt(), message);
    }

    public static void Error(this ILogger logger, Logs log, string message = "")
    {
        logger.Log(LogLevel.Error, log.ToInt(), message);
    }

    public static void Debug(this ILogger logger, Logs log, string message = "")
    {
        logger.Log(LogLevel.Debug, log.ToInt(), message);
    }
}