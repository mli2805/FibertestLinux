using Fibertest.Utils;
using Fibertest.Dto;
using Fibertest.Utils.Setup;

namespace Fibertest.Rtu;

public static class RtuConfigServiceCollectionExtension
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        var configFile = Path.Combine(FileOperations.GetMainFolder(), "config/rtu.json");

        services.ConfigureWritable<RtuGeneralConfig>(config.GetSection("General"), configFile);
        services.ConfigureWritable<CharonConfig>(config.GetSection("Charon"), configFile);
        services.ConfigureWritable<MonitoringConfig>(config.GetSection("Monitoring"), configFile);
        services.ConfigureWritable<RecoveryConfig>(config.GetSection("Recovery"), configFile);
        return services;
    }

    public static void Configure(this IConfigurationBuilder configurationBuilder)
    {
        var configFile = FileOperations.GetMainFolder() +"/config/rtu.json";

        ConfigUtils.EnsureCreation<RtuConfig>(configFile);
        configurationBuilder.AddJsonFile(configFile, false, true);
    }

}