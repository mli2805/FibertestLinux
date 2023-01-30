using System.Windows;
using System.Windows.Threading;

namespace Fibertest.WpfClient
{
    public class UiDispatcherProvider : IDispatcherProvider
    {
        public Dispatcher GetDispatcher() { return Application.Current.Dispatcher; }
    }
}