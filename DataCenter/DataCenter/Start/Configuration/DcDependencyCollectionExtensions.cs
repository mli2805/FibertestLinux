namespace Fibertest.DataCenter;

public static class DcDependencyCollectionExtensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        return services
            .AddBootAndBackgroundServices()
            .AddDbRepositories()
            .AddOther();
    }

    private static IServiceCollection AddBootAndBackgroundServices(this IServiceCollection services)
    {
        services.AddSingleton<Boot>();
        services.AddHostedService(x => x.GetService<Boot>());
        services.AddSingleton<MessageQueueService>();
        services.AddHostedService(x => x.GetService<MessageQueueService>());
        services.AddSingleton<SnmpTrapListenerService>();
        services.AddHostedService(x => x.GetService<SnmpTrapListenerService>());

        return services;
    }

    private static IServiceCollection AddDbRepositories(this IServiceCollection services)
    {
        services.AddScoped<RtuRepo>(); // для каждого реквеста новый
        services.AddSingleton<ClientCollection>();
        services.AddSingleton<RtuOccupations>();

        return services;
    }

    private static IServiceCollection AddOther(this IServiceCollection services)
    {
        services.AddSingleton<TrapDataProcessor>();

        return services;
    }

}