namespace Fibertest.DataCenter.Start;

public static class DependencyInjectionExtensions
{
    // методов может быть несколько
    // а названия описывать объединенные зависимости
    public static IServiceCollection AddDbRepositories(this IServiceCollection services)
    {
        services.AddScoped<RtuRepo>(); // для каждого реквеста новый
        // services.AddScoped<class2>();
        // services.AddScoped<class3>();

        return services;
    }
}