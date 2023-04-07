using System.Windows;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public partial class MainMenuViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly ComponentsReportViewModel _componentsReportViewModel;
        private readonly OpticalEventsReportViewModel _opticalEventsReportViewModel;
        private readonly TcesViewModel _tcesViewModel;

        private CurrentUser _currentUser = new CurrentUser();
        public CurrentUser CurrentUser
        {
            get => _currentUser;
            set
            {
                if (Equals(value, _currentUser)) return;
                _currentUser = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ToolsMenuVisibility));
            }
        }

        public Visibility ToolsMenuVisibility =>
            CurrentUser.Role == Role.Developer ? Visibility.Visible : Visibility.Collapsed;

        public MainMenuViewModel(ILifetimeScope globalScope, IWindowManager windowManager,
            ComponentsReportViewModel componentsReportViewModel, OpticalEventsReportViewModel opticalEventsReportViewModel,
            TcesViewModel tcesViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _componentsReportViewModel = componentsReportViewModel;
            _opticalEventsReportViewModel = opticalEventsReportViewModel;
            _tcesViewModel = tcesViewModel;
        }

     
    }
}