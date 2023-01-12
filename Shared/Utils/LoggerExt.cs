using Microsoft.Extensions.Logging;

namespace Fibertest.Utils;

public static class LoggerExt
{
    // timestamp without message AND then absolutely empty space
    public static void TimestampWithoutMessage<T>(this ILogger<T> logger, Logs log)
    {
        logger.Log(LogLevel.Information, log.ToInt(), "");
    }

    public static void HyphenLine<T>(this ILogger<T> logger, Logs log)
    {
        logger.Log(LogLevel.Information, log.ToInt(), new string('-', 78));
    }

    public static void StartLine<T>(this ILogger<T> logger, Logs log)
    {
        logger.Log(LogLevel.Information, log.ToInt(), Environment.NewLine + Environment.NewLine + new string('-', 78));
    }

    public static void EmptyAndLog<T>(this ILogger<T> logger, Logs log, string message = "")
    {
        logger.Log(LogLevel.Information, log.ToInt(), "");
        logger.Log(LogLevel.Information, log.ToInt(), message);
    }

    public static void LLog<T>(this ILogger<T> logger, Logs log, string message = "", LogLevel logLevel = LogLevel.Information)
    {
        logger.Log(logLevel, log.ToInt(), message);
    }

    public static void LogError<T>(this ILogger<T> logger, Logs log, string message = "")
    {
        logger.Log(LogLevel.Error, log.ToInt(), message);
    }

    public static void LogDebug<T>(this ILogger<T> logger, Logs log, string message = "")
    {
        logger.Log(LogLevel.Debug, log.ToInt(), message);
    }

    public static void LLog(this ILogger logger, Logs log, string message = "", LogLevel logLevel = LogLevel.Information)
    {
        logger.Log(logLevel, log.ToInt(), message);
    }

    public static void LogError(this ILogger logger, Logs log, string message = "")
    {
        logger.Log(LogLevel.Error, log.ToInt(), message);
    }

    public static void LogDebug(this ILogger logger, Logs log, string message = "")
    {
        logger.Log(LogLevel.Debug, log.ToInt(), message);
    }
}