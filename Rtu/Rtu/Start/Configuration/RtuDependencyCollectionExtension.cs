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
        return services;
    }

    private static IServiceCollection AddOther(this IServiceCollection services)
    {
        services.AddScoped<InterOpWrapper>(); // для каждого реквеста новый
        services.AddScoped<SerialPortManager>(); 
        services.AddScoped<OtdrManager>(); 
        services.AddScoped<RtuManager>(); 
        return services;
    }


}