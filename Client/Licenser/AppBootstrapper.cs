using System;
using System.Collections.Generic;
using Caliburn.Micro;

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

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) {
            DisplayRootViewForAsync<IShell>();
        }
    }
}
