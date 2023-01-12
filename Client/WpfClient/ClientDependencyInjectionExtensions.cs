using Caliburn.Micro;
using Fibertest.Utils;
using GrpsClientLib;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Fibertest.WpfClient;

public static class ClientDependencyInjectionExtensions
{
    public static void AddMyDependencies(this SimpleContainer container)
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddDebug();  // Debug.WriteLine() - see Output window
            builder.AddConsole(); // in WPF does not work !!!
            builder.AddSerilog(LoggerConfigurationFactory
                .Configure("2") // here is my configuration of log files
                .CreateLogger());
        });

        container
            .AddOneGroup(loggerFactory)
            .AddLastGroup(loggerFactory);
    }

    private static SimpleContainer AddOneGroup(this SimpleContainer container, ILoggerFactory lf)
    {
        container.Singleton<GrpcC2RRequests>().RegisterInstance(typeof(ILogger<GrpcC2RRequests>), "", lf.CreateLogger<GrpcC2RRequests>());

        return container;
    }

    private static void AddLastGroup(this SimpleContainer container, ILoggerFactory lf)
    {
        container.Singleton<GrpcC2DRequests>().RegisterInstance(typeof(ILogger<GrpcC2DRequests>), "", lf.CreateLogger<GrpcC2DRequests>());
    }
}