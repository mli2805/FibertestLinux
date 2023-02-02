// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Options;
//
// namespace Fibertest.Utils;
//
// public static class ServiceCollectionExtensions
// {
//     public static void ConfigureWritable<T>(
//         this IServiceCollection services,
//         IConfigurationSection section,
//         string file = "appsettings.json") where T : class, new()
//     {
//         services.Configure<T>(section);
//         services.AddTransient<IWritableOptions<T>>(provider =>
//         {
// #pragma warning disable CS8600
//             var configuration = (IConfigurationRoot)provider.GetService<IConfiguration>();
// #pragma warning restore CS8600
//             var options = provider.GetService<IOptionsMonitor<T>>();
// #pragma warning disable CS8604
//             return new WritableOptions<T>(options, configuration, section.Key, file);
// #pragma warning restore CS8604
//         });
//     }
// }