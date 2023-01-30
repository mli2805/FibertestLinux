namespace Fibertest.WpfClient
{
    public interface IClientWcfServiceHost
    {
        void StartWcfListener();
    }

    public class ClientWcfService : IClientWcfServiceHost
    {
        public void StartWcfListener()
        {
        }
    }
}