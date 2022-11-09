using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public class RtuRepo
    {
        private readonly ILogger<RtuRepo> _logger;

        public RtuRepo(ILogger<RtuRepo> logger)
        {
            _logger = logger;
        }

        public DoubleAddress? GetRtuAddresses(Guid rtuId)
        {
            try
            {
                // return new DoubleAddress() { Main = new NetAddress("localhost", 11942) };
                return new DoubleAddress() { Main = new NetAddress("192.168.96.56", 11942) };
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), $"GetRtuAddresses: {rtuId}" + e.Message);
                return null;
            }
        }
 
    }
}
