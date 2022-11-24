using Fibertest.Utils;

namespace Fibertest.DataCenter;

public static class DcConfigServiceCollectionExtension
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        services.ConfigureWritable<DataCenterConfig>(config.GetSection("General"), "../config/dc.json");
        services.ConfigureWritable<MysqlConfig>(config.GetSection("MySql"), "../config/dc.json");

        return services;
    }
}