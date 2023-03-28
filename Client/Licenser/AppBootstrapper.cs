using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Caliburn.Micro;
using Fibertest.WpfCommonViews;

namespace Licenser
{
    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

#pragma warning disable CS8618
        public AppBootstrapper() {
#pragma warning restore CS8618
            Initialize();
        }

        protected override void Configure() {
            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<IShell, ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key) {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance) {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.CurrentUICulture = new CultureInfo("ru-RU");

            DisplayRootViewForAsync<IShell>();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            yield return typeof(ShellView).Assembly; // this Assembly (.exe)
            yield return typeof(IWaitCursor).Assembly; // WpfCommonViews
        }
    }
}
