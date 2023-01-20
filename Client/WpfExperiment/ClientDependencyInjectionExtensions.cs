﻿using Caliburn.Micro;
using Fibertest.Utils;
using GrpsClientLib;
using Microsoft.Extensions.Logging;
using Serilog;
using Fibertest.Dto;

namespace WpfExperiment;

public static class ClientDependencyInjectionExtensions
{
    public static void AddMyDependencies(this SimpleContainer container)
    {
        container
            .AddConfig()
            .AddLogging()
            .AddOtherDependencies();
    }

    private static SimpleContainer AddConfig(this SimpleContainer container)
    {
        var clientConfig = new WritableConfig<ClientConfig>("wpfExp.json");
        container.RegisterInstance(typeof(IWritableConfig<ClientConfig>), "", clientConfig);
        return container;
    }

    private static SimpleContainer AddLogging(this SimpleContainer container)
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

        Microsoft.Extensions.Logging.ILogger implementation = loggerFactory.CreateLogger<ShellViewModel>();
        container.RegisterInstance(typeof(Microsoft.Extensions.Logging.ILogger), "", implementation);

        return container;
    }

    private static void AddOtherDependencies(this SimpleContainer container)
    {
        container.Singleton<GrpcC2RRequests>();
        container.Singleton<GrpcC2DRequests>();
    }
}