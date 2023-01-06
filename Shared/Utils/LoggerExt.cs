using Microsoft.Extensions.Logging;

namespace Fibertest.Utils
{
    public static class LoggerExt
    {
        // timestamp without message AND then absolutely empty space
        public static void EmptyLine<T>(this ILogger<T> logger, EventId eventId)
        {
            logger.Log(LogLevel.Information, eventId, Environment.NewLine);
        }

        public static void HyphenLine<T>(this ILogger<T> logger, EventId eventId)
        {
            logger.Log(LogLevel.Information, eventId, new string('-', 78));
        }

        public static void StartLine<T>(this ILogger<T> logger, EventId eventId)
        {
            logger.Log(LogLevel.Information, eventId, Environment.NewLine + Environment.NewLine + new string('-', 78));
        }

        public static void LLog<T>(this ILogger<T> logger, EventId eventId, string message = "", LogLevel logLevel = LogLevel.Information)
        {
            logger.Log(logLevel, eventId, message);
        }
        
        public static void LLog(this ILogger logger, EventId eventId, string message = "", LogLevel logLevel = LogLevel.Information)
        {
            logger.Log(logLevel, eventId, message);
        }
    }
}
