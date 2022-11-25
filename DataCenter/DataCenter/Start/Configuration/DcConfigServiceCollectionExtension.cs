using System.Reflection;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public static class DcConfigServiceCollectionExtension
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var basePath = Path.GetDirectoryName(assemblyLocation) ?? "";
        var configFile = Path.Combine(basePath, @"../config/dc.json");
        
        services.ConfigureWritable<DataCenterConfig>(config.GetSection("General"), configFile);
        services.ConfigureWritable<MysqlConfig>(config.GetSection("MySql"), configFile);

        return services;
    }
}