using Fibertest.Utils;
using Fibertest.Dto;
using Fibertest.Utils.Setup;

namespace Fibertest.Rtu;

public static class RtuConfigServiceCollectionExtension
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        var configFile = Path.Combine(FileOperations.GetFibertestFolder(), "config/rtu.json");

        services.ConfigureWritable<RtuGeneralConfig>(config.GetSection("General"), configFile);
        services.ConfigureWritable<CharonConfig>(config.GetSection("Charon"), configFile);
        services.ConfigureWritable<MonitoringConfig>(config.GetSection("Monitoring"), configFile);
        services.ConfigureWritable<RecoveryConfig>(config.GetSection("Recovery"), configFile);
        return services;
    }

    public static void Configure(this IConfigurationBuilder configurationBuilder)
    {
        var configFile = FileOperations.GetFibertestFolder() +"/config/rtu.json";

        ConfigUtils.Validate(configFile, new RtuConfig());
        configurationBuilder.AddJsonFile(configFile, false, true);
    }

}