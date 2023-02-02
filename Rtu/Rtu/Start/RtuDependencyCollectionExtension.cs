using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public static class RtuDependencyCollectionExtension
{
    public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
    {
        return services
            .AddConfigAsInstance()
            .AddBootAndBackgroundServices()
            .AddOther();
    }

    private static IServiceCollection AddConfigAsInstance(this IServiceCollection services)
    {
        return services
            .AddSingleton<IWritableConfig<RtuConfig>>(_ => new WritableConfig<RtuConfig>("rtu.json"));
    }
    private static IServiceCollection AddBootAndBackgroundServices(this IServiceCollection services)
    {
        services.AddSingleton<Boot>();
        services.AddHostedService(x => x.GetService<Boot>());
        services.AddSingleton<MonitoringService>();
        services.AddHostedService(x => x.GetService<MonitoringService>());
        services.AddSingleton<HeartbeatService>();
        services.AddHostedService(x => x.GetService<HeartbeatService>());
        return services;
    }

    private static IServiceCollection AddOther(this IServiceCollection services)
    {
        services.AddSingleton<GrpcSender>();

        services.AddSingleton<InterOpWrapper>();
        services.AddSingleton<SerialPortManager>();
        services.AddSingleton<MonitoringQueue>();
        services.AddSingleton<OtdrManager>();
        services.AddSingleton<RtuManager>();
        return services;
    }


}