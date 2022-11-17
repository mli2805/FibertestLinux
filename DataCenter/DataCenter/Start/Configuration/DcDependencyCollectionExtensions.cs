namespace Fibertest.DataCenter;

public static class DcDependencyCollectionExtensions
{
    // методов может быть несколько
    // а названия описывать объединенные зависимости
    public static IServiceCollection AddDbRepositories(this IServiceCollection services)
    {
        services.AddScoped<RtuRepo>(); // для каждого реквеста новый
        services.AddSingleton<ClientCollection>();
        services.AddSingleton<RtuOccupations>();

        return services;
    }
}