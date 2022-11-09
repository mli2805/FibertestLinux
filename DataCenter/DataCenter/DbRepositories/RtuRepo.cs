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

        /// <summary>
        /// DB stores RTU channels availability, so:
        /// 
        /// returns first available RTU address: main or reserve
        /// if neither - returns null
        /// </summary>
        /// <param name="rtuId"></param>
        /// <returns></returns>
        public string? GetRtuAvailableAddress(Guid rtuId)
        {
            try
            {
                // return "localhost:11942";
                return "192.168.96.56:11942";
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), $"GetRtuAvailableAddress: {rtuId}" + e.Message);
                return null;
            }
        }
 
    }
}
