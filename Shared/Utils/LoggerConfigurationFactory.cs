using Serilog.Events;
using Serilog;
using System.Diagnostics;

namespace Fibertest.Utils;

// Do not use small numbers - they are used by MS already!
public enum Logs
{
    RtuService = 513248,
    RtuManager = 715343,
    SnmpTraps = 231748,
    DataCenter = 187901,
    WebApi = 625487,
    Client = 351458,

}

public static class LogsExt
{
    public static int ToInt(this Logs log)
    {
        return (int)log;
    }
}

/// <summary>
///  provides way to persist messages from one project into several files according to EventId
/// </summary>
public static class LoggerConfigurationFactory
{
    public static LoggerConfiguration Configure()
    {
        var template = "[{Timestamp:HH:mm:ss} {CorrelationId} {Level:u3}] {Username} {Message:lj}{NewLine}{Exception}";
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Logger(cc => cc
                .Filter.ByIncludingOnly(WithEventId(Logs.Client.ToInt()))
                .WriteTo
                .File("../log/cl-.log", outputTemplate: template,
                    rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromSeconds(1)))
            .WriteTo.Logger(cc => cc
                .Filter.ByIncludingOnly(WithEventId(Logs.DataCenter.ToInt()))
                .WriteTo
                .File("../log/dc-.log", outputTemplate: template,
                    rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromSeconds(1)))
            .WriteTo.Logger(cc => cc
                .Filter.ByIncludingOnly(WithEventId(Logs.SnmpTraps.ToInt()))
                .WriteTo
                .File("../log/trap-.log", outputTemplate: template,
                    rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromSeconds(1)))
            .WriteTo.Logger(cc => cc
                .Filter.ByIncludingOnly(WithEventId(Logs.RtuService.ToInt()))
                .WriteTo
                .File("../log/srv-.log", outputTemplate: template,
                    rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromSeconds(1)))
            .WriteTo.Logger(cc => cc
                .Filter.ByIncludingOnly(WithEventId(Logs.RtuManager.ToInt()))
                .WriteTo
                .File("../log/mng-.log", outputTemplate: template,
                    rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromSeconds(1)))
            ;

        if (Debugger.IsAttached)
        {
            loggerConfiguration
                .WriteTo.Logger(cc => cc
                    .Filter.ByExcluding(NotMine())
                    .WriteTo.Console());
        }

        return loggerConfiguration;
    }

    private static Func<LogEvent, bool> NotMine()
    {
        return e =>
        {
            if (e.Properties.TryGetValue("EventId", out var propertyValue))
            {
                if (propertyValue is StructureValue stValue)
                {
                    var value = stValue.Properties.FirstOrDefault(cc => cc.Name == "Id");
                    if (value == null) return false;

                    foreach (var log in (Logs[])Enum.GetValues(typeof(Logs)))
                    {
                        ScalarValue scalar = new ScalarValue(log.ToInt());
                        if (scalar.Equals(value.Value))
                            return true;
                    }

                    return false;
                }
            }
            return false;
        };
    }


    private static Func<LogEvent, bool> WithEventId(object scalarValue)
    {
        ScalarValue scalar = new ScalarValue(scalarValue);
        return e =>
        {
            if (e.Properties.TryGetValue("EventId", out var propertyValue))
            {
                if (propertyValue is StructureValue stValue)
                {
                    var value = stValue.Properties.FirstOrDefault(cc => cc.Name == "Id");
                    if (value == null) return false;
                    bool result = scalar.Equals(value.Value);
                    return result;
                }
            }
            return false;
        };
    }
}
