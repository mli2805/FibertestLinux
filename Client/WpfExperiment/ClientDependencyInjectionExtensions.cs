using Caliburn.Micro;
using Fibertest.Utils;
using GrpsClientLib;
using Microsoft.Extensions.Logging;
using Serilog;

namespace WpfExperiment;

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
                .Configure() // here is my configuration of log files
                .CreateLogger());
        });

        container
            .AddOneGroup(loggerFactory)
            .AddLastGroup();
    }

    private static SimpleContainer AddOneGroup(this SimpleContainer container, ILoggerFactory lf)
    {
        container.Singleton<Class1>().RegisterInstance(typeof(ILogger<Class1>), "", lf.CreateLogger<Class1>());
        container.Singleton<Class2>().RegisterInstance(typeof(ILogger<Class2>), "", lf.CreateLogger<Class2>());

        return container;
    }

    private static void AddLastGroup(this SimpleContainer container)
    {
        container.Singleton<Class3>();
    }
}