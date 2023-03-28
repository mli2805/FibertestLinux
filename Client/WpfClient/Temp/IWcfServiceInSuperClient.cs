using System.Threading.Tasks;

namespace Fibertest.WpfClient
{
    public interface IWcfServiceInSuperClient
    {
        Task<int> ClientLoadingResult(int postfix, bool isLoadedOk, bool isStateOk);
        Task<int> NotifyConnectionBroken(int postfix);
        Task<int> ClientClosed(int postfix);
        Task<int> SetSystemState(int postfix, bool isStateOk);
        Task<int> SwitchOntoSystem(int postfix);
    }

    public class WcfServiceInSuperClient : IWcfServiceInSuperClient
    {
        public Task<int> ClientLoadingResult(int postfix, bool isLoadedOk, bool isStateOk)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> NotifyConnectionBroken(int postfix)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> ClientClosed(int postfix)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> SetSystemState(int postfix, bool isStateOk)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> SwitchOntoSystem(int postfix)
        {
            throw new System.NotImplementedException();
        }
    }
}