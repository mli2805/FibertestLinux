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
}