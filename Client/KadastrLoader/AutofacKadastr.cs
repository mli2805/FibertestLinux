using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using GrpsClientLib;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace KadastrLoader
{
    public sealed class AutofacKadastr : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var clientConfig = new WritableConfig<ClientConfig>("kadastr.json");
            builder.RegisterInstance<IWritableConfig<ClientConfig>>(clientConfig);

            ILoggerFactory loggerFactory = LoggerFactory.Create(b =>
            {
                b.ClearProviders();
                b.AddDebug();  // Debug.WriteLine() - see Output window
                b.AddConsole(); // in WPF does not work !!!
                b.AddSerilog(LoggerConfigurationFactory
                    .Configure(LogEventLevel.Information) // here is my configuration of log files
                    .CreateLogger());
            });

            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger<KadastrLoaderViewModel>();
            builder.RegisterInstance(logger).As<Microsoft.Extensions.Logging.ILogger>();

            builder.RegisterType<KadastrLoaderViewModel>().As<IShell>();
            builder.RegisterType<WindowManager>().As<IWindowManager>().InstancePerLifetimeScope();


            builder.RegisterType<WaitCursor>().As<IWaitCursor>().InstancePerLifetimeScope();
            builder.RegisterType<GrpcC2DService>().InstancePerLifetimeScope();
            builder.RegisterType<GrpcC2DService>().InstancePerLifetimeScope();

            builder.RegisterType<KadastrDbProvider>().InstancePerLifetimeScope();
            builder.RegisterType<KadastrDbSettings>().InstancePerLifetimeScope();

            builder.RegisterInstance(new LoadedAlready());

            builder.RegisterType<KadastrFilesParser>().InstancePerLifetimeScope();
            builder.RegisterType<ChannelParser>().InstancePerLifetimeScope();
            builder.RegisterType<WellParser>().InstancePerLifetimeScope();
            builder.RegisterType<ConpointParser>().InstancePerLifetimeScope();
        }
    }
}
