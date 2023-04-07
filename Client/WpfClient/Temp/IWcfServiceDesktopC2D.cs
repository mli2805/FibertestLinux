using System.Threading.Tasks;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public interface IWcfServiceDesktopC2D
    {
        #region Settings
       
        Task<bool> SendTest(string to, NotificationType notificationType);
        #endregion
    }

    public class WcfServiceDesktopC2D : IWcfServiceDesktopC2D
    {
        
        public Task<bool> SendTest(string to, NotificationType notificationType)
        {
            throw new System.NotImplementedException();
        }
    }
}