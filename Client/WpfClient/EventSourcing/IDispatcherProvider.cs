using System.Windows.Threading;

namespace Fibertest.WpfClient
{
    public interface IDispatcherProvider { Dispatcher GetDispatcher(); }
}