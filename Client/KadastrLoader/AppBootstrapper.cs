using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using Autofac;
using Caliburn.Micro;
using Fibertest.WpfCommonViews;


namespace KadastrLoader
{
    public class AppBootstrapper : BootstrapperBase
    {
        private ILifetimeScope _container;

#pragma warning disable CS8618
        public AppBootstrapper() {
#pragma warning restore CS8618
            Initialize();
        }

        protected override void Configure()  { }

        protected override object GetInstance(Type service, string key) {
            return string.IsNullOrWhiteSpace(key) ?
                _container.Resolve(service) :
                _container.ResolveNamed(key, service);
        }

        protected override IEnumerable<object>? GetAllInstances(Type service) {
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance) {
            _container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) 
        {
            SomeInitialActions();

            DisplayRootViewForAsync<IShell>();
        }

        private void SomeInitialActions()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacKadastr>();
            _container = builder.Build();

            var cultureInfo = new CultureInfo("ru-RU") { NumberFormat = { NumberDecimalSeparator = @"." } };
            // Ensure the current culture passed into bindings 
            // is the OS culture. By default, WPF uses en-US 
            // as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag)));
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            yield return typeof(KadastrLoaderView).Assembly; // this Assembly (.exe)
            yield return typeof(RftsEventsView).Assembly; // WpfCommonViews
        }
    }
}
