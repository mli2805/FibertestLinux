using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient;

public class AppBootstrapper : BootstrapperBase
{
    private ILifetimeScope _container;

#pragma warning disable CS8618
    public AppBootstrapper()
    {
#pragma warning restore CS8618
        Initialize();
    }

    // from Caliburn.Micro example
    protected override void Configure()
    {
        var defaultCreateTrigger = Parser.CreateTrigger;

        Parser.CreateTrigger = (target, triggerText) =>
        {
            if (triggerText == null)
            {
                return defaultCreateTrigger(target, null);
            }

            var triggerDetail = triggerText
                .Replace(@"[", string.Empty)
                .Replace(@"]", string.Empty);

            var splits = triggerDetail.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            switch (splits[0])
            {
                case "Key":
                    var key = (Key)Enum.Parse(typeof(Key), splits[1], true);
                    return new KeyTrigger { Key = key };

                case "Gesture":
                    var mkg = (MultiKeyGesture?)new MultiKeyGestureConverter().ConvertFrom(splits[1]);
                    if (mkg == null) return null;
                    return new KeyTrigger { Modifiers = mkg.KeySequences[0].Modifiers, Key = mkg.KeySequences[0].Keys[0] };
            }

            return defaultCreateTrigger(target, triggerText);
        };
    }

    protected override object GetInstance(Type service, string key)
    {
        return string.IsNullOrWhiteSpace(key) ?
            _container.Resolve(service) :
            _container.ResolveNamed(key, service);
    }

    protected override IEnumerable<object>? GetAllInstances(Type service)
    {
        return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
    }

    protected override void BuildUp(object instance)
    {
        _container.InjectProperties(instance);
    }

    protected override void OnStartup(object sender, StartupEventArgs e)
    {
        SomeInitialActions(e.Args);
        DisplayRootViewForAsync<IShell>();
    }

    private void SomeInitialActions(string[] commandLineParams)
    {
        var postfix = commandLineParams.Length == 0 ? "" : commandLineParams[0];

        var thisProcessName = Process.GetCurrentProcess().ProcessName;
        if (postfix == "" && Process.GetProcesses().Count(p => p.ProcessName == thisProcessName) > 1)
            Environment.FailFast(@"Fast termination of application.");

        var configFileName = $@"client{postfix}.json";

        var builder = new ContainerBuilder();
        builder.RegisterModule<AutofacClient>();

        var clientConfig = new WritableConfig<ClientConfig>(configFileName);
        builder.RegisterInstance<IWritableConfig<ClientConfig>>(clientConfig);

        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
        clientConfig.Update(c => c.General.Version = info.FileVersion!);

        var parameters = ParseCommandLine(commandLineParams);
        builder.RegisterInstance(parameters);
        _container = builder.Build();

        var currentCulture = postfix == ""
            ? clientConfig.Value.General.Culture
            : parameters.SuperClientCulture;
        var cultureInfo = new CultureInfo(currentCulture) { NumberFormat = { NumberDecimalSeparator = @"." } };
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;

        // Ensure the current culture passed into bindings 
        // is the OS culture. By default, WPF uses en-US 
        // as the culture, regardless of the system settings.
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag)));
    }

    private CommandLineParameters ParseCommandLine(string[] args)
    {
        var parameters = new CommandLineParameters();
        parameters.IsUnderSuperClientStart = args.Length != 0;
        if (parameters.IsUnderSuperClientStart)
        {
            if (int.TryParse(args[0], out int clientOrdinal))
                parameters.ClientOrdinal = clientOrdinal;
            parameters.SuperClientCulture = args[1];
            if (args.Length >= 5)
            {
                parameters.Username = args[2];
                parameters.Password = args[3];
                parameters.ConnectionId = args[4];
            }

            if (args.Length == 8)
            {
                if (int.TryParse(args[6], out int serverPort))
                    parameters.ServerNetAddress = new NetAddress(args[5], serverPort);
                parameters.ServerTitle = args[7];
            }
        }
        return parameters;
    }

    protected override IEnumerable<Assembly> SelectAssemblies()
    {
        yield return typeof(ShellView).Assembly; // this Assembly (.exe)
        yield return typeof(RftsEventsView).Assembly; // WpfCommonViews
    }
}