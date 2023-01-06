using System.Globalization;
using System.Text;
using Fibertest.Dto;
using Microsoft.Extensions.Logging;
using SnmpSharpNet;

namespace Fibertest.Utils.Snmp
{
    public class SnmpAgent
    {
        private readonly IWritableOptions<SnmpConfig> _config;
        private readonly ILogger<SnmpAgent> _logger;

        public SnmpAgent(IWritableOptions<SnmpConfig> config, ILogger<SnmpAgent> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void SaveSnmpSettings(SnmpConfig dto)
        {
            _config.Update(c =>c.FillIn(dto));
        }

        public bool SendTestTrap()
        {
            var trapData = CreateTestTrapData();
            if (_config.Value.SnmpTrapVersion == "v1")
                return SendSnmpV1Trap(trapData, FtTrapType.TestTrap);
            return false;
        }

        private bool SendSnmpV1Trap(VbCollection trapData, FtTrapType trapType)
        {
            try
            {
                TrapAgent trapAgent = new TrapAgent();
                trapAgent.SendV1Trap(new IpAddress(_config.Value.SnmpReceiverIp),
                    _config.Value.SnmpReceiverPort,
                    _config.Value.SnmpCommunity,
                    new Oid(_config.Value.EnterpriseOid),
                    new IpAddress(_config.Value.SnmpAgentIp),
                    6,
                    (int)trapType, // my trap type 
                    12345678, // system UpTime in 0,1sec
                    trapData);
                _logger.LLog(Logs.DataCenter.ToInt(), "SendSnmpV1Trap sent.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), $"SendSnmpV1Trap: {e.Message}");
                return false;
            }
        }

        public bool SendSnmpV2CTrap(VbCollection trapData, uint upTime, Oid trapObjOid)
        {
            try
            {
                TrapAgent trapAgent = new TrapAgent();
                trapAgent.SendV2Trap(new IpAddress(_config.Value.SnmpReceiverIp),
                    _config.Value.SnmpReceiverPort,
                    "public",
                    upTime,
                    trapObjOid,
                    trapData);
                _logger.LLog(Logs.DataCenter.ToInt(), "SendSnmpV2Trap sent.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), $"SendSnmpV2Trap: {e.Message}");
                return false;
            }
        }

        public bool SentRealTrap(List<KeyValuePair<FtTrapProperty, string>> data, FtTrapType trapType)
        {
            var trapData = new VbCollection();
            foreach (KeyValuePair<FtTrapProperty, string> pair in data)
            {
                trapData.Add(new Oid(_config.Value.EnterpriseOid + "." + (int)pair.Key),
                    new OctetString(EncodeString(pair.Value, _config.Value.SnmpEncoding)));
            }
            return SendSnmpV1Trap(trapData, trapType);
        }

        private VbCollection CreateTestTrapData()
        {
            var trapData = new VbCollection();

            trapData.Add(new Oid(_config.Value.EnterpriseOid + "." + (int)FtTrapProperty.TestString),
                new OctetString(EncodeString("Test string with Русский язык.", _config.Value.SnmpEncoding)));
            trapData.Add(new Oid(_config.Value.EnterpriseOid + "." + (int)FtTrapProperty.EventRegistrationTime),
                new OctetString(DateTime.Now.ToString("G")));
            trapData.Add(new Oid(_config.Value.EnterpriseOid + "." + (int)FtTrapProperty.TestInt), new Integer32(412));
            var doubleValue = 43.0319;
            trapData.Add(new Oid(_config.Value.EnterpriseOid + "." + (int)FtTrapProperty.TestDouble),
                new OctetString(doubleValue.ToString(CultureInfo.CurrentUICulture)));
            return trapData;
        }

        private byte[] EncodeString(string str, string encondingName)
        {
            switch (encondingName.ToLower())
            {
                case "unicode":
                    var unicodeEncoding = new UnicodeEncoding();
                    return unicodeEncoding.GetBytes(str);
                case "windows1251":
                    var win1251Encoding = Encoding.GetEncoding("windows-1251");
                    return win1251Encoding.GetBytes(str);
                // ReSharper disable once RedundantCaseLabel
                case "utf8":
                default:
                    var utf8Encoding = new UTF8Encoding();
                    return utf8Encoding.GetBytes(str);
            }
        }
    }
}
