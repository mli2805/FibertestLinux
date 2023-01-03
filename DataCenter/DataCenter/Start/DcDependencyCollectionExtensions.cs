using Fibertest.Graph;

namespace Fibertest.DataCenter;

public static class DcDependencyCollectionExtensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        return services
            .AddBootAndBackgroundServices()
            .AddGlobalVars()
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

    private static IServiceCollection AddGlobalVars(this IServiceCollection services)
    {
        services.AddSingleton<ClientCollection>();
        services.AddSingleton<RtuOccupations>();
        return services;
    }
  private static IServiceCollection AddDbRepositories(this IServiceCollection services)
    {
        services.AddScoped<RtuStationsRepository>(); // для каждого реквеста новый
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

        services.AddSingleton<ClientToIitRtuTransmitter>();
        services.AddScoped<IntermediateClass>();
        services.AddSingleton<RtuInitializationToGraphApplier>();

        return services;
    }

}