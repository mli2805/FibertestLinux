using System.Windows.Threading;

namespace Fibertest.WpfClient
{
    public class TestsDispatcherProvider : IDispatcherProvider
    {
        public Dispatcher GetDispatcher() { return Dispatcher.CurrentDispatcher; }
    }
}