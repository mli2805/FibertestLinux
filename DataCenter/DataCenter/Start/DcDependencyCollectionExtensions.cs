using Fibertest.Graph;

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

        return services;
    }

    private static IServiceCollection AddDbRepositories(this IServiceCollection services)
    {
        services.AddScoped<RtuRepo>(); // для каждого реквеста новый
        services.AddSingleton<ClientCollection>();
        services.AddSingleton<RtuOccupations>();

        services.AddSingleton<SnapshotRepository>();


        return services;
    }

    private static IServiceCollection AddOther(this IServiceCollection services)
    {
        services.AddSingleton<ClientGrpcRequestExecutor>();
        services.AddSingleton<IDbInitializer, MySqlDbInitializer>();

        services.AddSingleton<MySerializer>();
        services.AddSingleton<EventsQueue>();
        services.AddSingleton<CommandAggregator>();
        services.AddSingleton<EventStoreService>();


        services.AddSingleton<Model>();
        services.AddSingleton<EventToLogLineParser>();
        services.AddSingleton<EventLogComposer>();

        return services;
    }

}