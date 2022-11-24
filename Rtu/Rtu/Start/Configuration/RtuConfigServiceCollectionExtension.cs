using Fibertest.Utils;

namespace Fibertest.Rtu;

public static class RtuConfigServiceCollectionExtension
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        services.ConfigureWritable<RtuConfig>(config.GetSection("General"), "../config/rtu.json");
        services.ConfigureWritable<CharonConfig>(config.GetSection("Charon"),"../config/rtu.json");

        return services;
    }
}