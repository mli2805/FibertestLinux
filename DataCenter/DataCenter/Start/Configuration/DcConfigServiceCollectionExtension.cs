using System.Reflection;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public static class DcConfigServiceCollectionExtension
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        var configFile = GetConfigPath();
        
        services.ConfigureWritable<ServerGeneralConfig>(config.GetSection("General"), configFile);
        services.ConfigureWritable<BroadcastConfig>(config.GetSection("Broadcast"), configFile);
        services.ConfigureWritable<ServerTimeoutConfig>(config.GetSection("ServerTimeouts"), configFile);
        services.ConfigureWritable<MysqlConfig>(config.GetSection("MySql"), configFile);
        services.ConfigureWritable<EventSourcingConfig>(config.GetSection("EventSourcing"), configFile);
        services.ConfigureWritable<SmtpConfig>(config.GetSection("Smtp"), configFile);
        services.ConfigureWritable<SnmpConfig>(config.GetSection("Snmp"), configFile);
        services.ConfigureWritable<WebApiConfig>(config.GetSection("WebApi"), configFile);

        return services;
    }

    public static void Configure(this IConfigurationBuilder configurationBuilder)
    {
        var configFile = GetConfigPath();

        ConfigValidator.Validate(configFile, new DataCenterConfig());
        configurationBuilder.AddJsonFile(configFile, false, true);
    }

    private static string GetConfigPath()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var basePath = Path.GetDirectoryName(assemblyLocation) ?? "";
        return Path.Combine(basePath, @"../config/dc.json");
    }
}