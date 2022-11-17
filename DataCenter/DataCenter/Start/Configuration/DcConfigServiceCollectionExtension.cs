using Microsoft.Extensions.Options;
#pragma warning disable CS8600
#pragma warning disable CS8604

namespace Fibertest.DataCenter;

public static class DcConfigServiceCollectionExtension
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        // services.Configure<DataCenterConfig>(config.GetSection("General"));
        // services.Configure<MysqlConfig>(config.GetSection("MySql"));

        services.ConfigureWritable<DataCenterConfig>(config.GetSection("General"), "dcconfig.json");
        services.ConfigureWritable<MysqlConfig>(config.GetSection("MySql"), "dcconfig.json");

        return services;
    }
}

public static class ServiceCollectionExtensions
{
    public static void ConfigureWritable<T>(
        this IServiceCollection services,
        IConfigurationSection section,
        string file = "appsettings.json") where T : class, new()
    {
        services.Configure<T>(section);
        services.AddTransient<IWritableOptions<T>>(provider =>
        {
            var configuration = (IConfigurationRoot)provider.GetService<IConfiguration>();
            var environment = provider.GetService<IWebHostEnvironment>();
            var options = provider.GetService<IOptionsMonitor<T>>();
            return new WritableOptions<T>(environment, options, configuration, section.Key, file);
        });
    }
}