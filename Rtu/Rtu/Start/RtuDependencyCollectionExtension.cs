using Fibertest.Utils;

namespace Fibertest.Rtu;

public static class RtuDependencyCollectionExtension
{
    public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
    {
          return services
              .AddBootAndBackgroundServices()
              .AddOther();
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
        services.AddSingleton<OtdrManager>(); 
        services.AddSingleton<RtuManager>(); 
        return services;
    }


}