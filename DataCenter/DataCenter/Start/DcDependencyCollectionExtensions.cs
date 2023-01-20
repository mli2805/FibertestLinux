using Fibertest.Graph;
using Fibertest.Utils.Snmp;

namespace Fibertest.DataCenter;

public static class DcDependencyCollectionExtensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        return services
            .AddBootAndBackgroundServices()
            .AddGlobalVars()
            .AddDbRepositories()
            .AddNotifiers()
            .AddOther();
    }

    private static IServiceCollection AddBootAndBackgroundServices(this IServiceCollection services)
    {
        services.AddSingleton<Boot>();
        services.AddHostedService(x => x.GetService<Boot>());
        services.AddSingleton<LastConnectionTimeChecker>();
        services.AddHostedService(x => x.GetService<LastConnectionTimeChecker>());
        services.AddSingleton<MessageQueueService>();
        services.AddHostedService(x => x.GetService<MessageQueueService>());

        return services;
    }

    private static IServiceCollection AddGlobalVars(this IServiceCollection services)
    {
        services.AddSingleton<GlobalState>();
        services.AddSingleton<ClientCollection>();
        services.AddSingleton<RtuOccupations>();
        services.AddSingleton<Model>();
        return services;
    }

    private static IServiceCollection AddDbRepositories(this IServiceCollection services)
    {
        services.AddSingleton<RtuStationsRepository>();
        services.AddSingleton<SorFileRepository>();
        services.AddSingleton<SnapshotRepository>();
        return services;
    }

    private static IServiceCollection AddNotifiers(this IServiceCollection services)
    {
        services.AddSingleton<IFtSignalRClient, FtSignalRClient>();

        services.AddSingleton<SmtpNotifier>();
        services.AddSingleton<SnmpAgent>();
        services.AddSingleton<SnmpNotifier>();

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

        services.AddSingleton<EventToLogLineParser>();
        services.AddSingleton<EventLogComposer>();

        services.AddSingleton<GraphGpsCalculator>();

        services.AddSingleton<BaseRefsCheckerOnServer>();
        services.AddSingleton<BaseRefLandmarksTool>();
        services.AddSingleton<TraceModelBuilder>();

        services.AddSingleton<C2RCommandsProcessor>();
        services.AddSingleton<ClientToIitRtuTransmitter>();
        services.AddSingleton<RtuResponseApplier>();
        services.AddSingleton<RtuResponseToEventSourcing>();

        return services;
    }

}