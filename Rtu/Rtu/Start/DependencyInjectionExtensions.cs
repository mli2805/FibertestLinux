namespace Fibertest.Rtu
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
        {
            services.AddScoped<Interop>(); // для каждого реквеста новый
            services.AddScoped<OtdrManager>(); 
            return services;

        }
    }
}
