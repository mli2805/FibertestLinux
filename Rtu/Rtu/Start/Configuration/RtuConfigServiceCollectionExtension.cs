using Fibertest.Utils;
using System.Reflection;
using Fibertest.Dto;

namespace Fibertest.Rtu;

public static class RtuConfigServiceCollectionExtension
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var basePath = Path.GetDirectoryName(assemblyLocation) ?? "";
        var configFile = Path.Combine(basePath, @"../config/rtu.json");
       
        services.ConfigureWritable<RtuGeneralConfig>(config.GetSection("General"), configFile);
        services.ConfigureWritable<CharonConfig>(config.GetSection("Charon"), configFile);
        services.ConfigureWritable<MonitoringConfig>(config.GetSection("Monitoring"), configFile);
        services.ConfigureWritable<RecoveryConfig>(config.GetSection("Recovery"), configFile);
        return services;
    }
}