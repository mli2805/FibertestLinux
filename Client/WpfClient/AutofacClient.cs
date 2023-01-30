using Autofac;
using Caliburn.Micro;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using GrpsClientLib;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Fibertest.WpfClient
{
    public sealed class AutofacClient : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(b =>
            {
                b.ClearProviders();
                b.AddDebug();  // Debug.WriteLine() - see Output window
                b.AddConsole(); // in WPF does not work !!!
                b.AddSerilog(LoggerConfigurationFactory
                    .Configure("2") // here is my configuration of log files
                    .CreateLogger());
            });

            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger<ShellViewModel>();
            builder.RegisterInstance(logger).As<Microsoft.Extensions.Logging.ILogger>();

            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<WindowManager>().As<IWindowManager>().InstancePerLifetimeScope();

            builder.RegisterType<WaitCursor>().As<IWaitCursor>().InstancePerLifetimeScope();
            builder.RegisterType<GrpcC2DRequests>().InstancePerLifetimeScope();
            builder.RegisterType<GrpcC2DRequests>().InstancePerLifetimeScope();

        }
    }
}
