using Fibertest.Utils;
using System.Reflection;

namespace Fibertest.Rtu;

public static class RtuConfigServiceCollectionExtension
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var basePath = Path.GetDirectoryName(assemblyLocation) ?? "";
        var configFile = Path.Combine(basePath, @"../config/rtu.json");
       
        services.ConfigureWritable<RtuConfig>(config.GetSection("General"), configFile); // RtuConfig class
        services.ConfigureWritable<CharonConfig>(config.GetSection("Charon"), configFile);

        return services;
    }
}