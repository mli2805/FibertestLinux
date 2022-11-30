namespace Fibertest.Rtu;

public static class RtuDependencyCollectionExtension
{
    public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
    {
        services.AddScoped<Interop>(); // для каждого реквеста новый
        services.AddScoped<SerialPortManager>(); 
        services.AddScoped<OtdrManager>(); 
        services.AddScoped<RtuManager>(); 
        return services;
    }
}