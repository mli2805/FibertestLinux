namespace Fibertest.Rtu;

public static class RtuConfigServiceCollectionExtension
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RtuConfig>(config.GetSection("General"));
        services.Configure<CharonConfig>(config.GetSection("Charon"));

        return services;
    }
}