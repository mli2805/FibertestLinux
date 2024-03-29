﻿using System.Globalization;
using System.Text;
using Fibertest.Dto;
using Microsoft.Extensions.Logging;
using SnmpSharpNet;

namespace Fibertest.Utils.Snmp;

public class SnmpAgent
{
    private readonly IWritableConfig<DataCenterConfig> _config;
    private readonly ILogger<SnmpAgent> _logger;

    public SnmpAgent(IWritableConfig<DataCenterConfig> config, ILogger<SnmpAgent> logger)
    {
        _config = config;
        _logger = logger;
    }

    public void SaveSnmpSettings(SnmpConfig dto)
    {
        _config.Update(c =>c.Snmp.FillIn(dto));
    }

    public bool SendTestTrap()
    {
        var trapData = CreateTestTrapData();
        if (_config.Value.Snmp.SnmpTrapVersion == "v1")
            return SendSnmpV1Trap(trapData, FtTrapType.TestTrap);
        return false;
    }

    private bool SendSnmpV1Trap(VbCollection trapData, FtTrapType trapType)
    {
        try
        {
            TrapAgent trapAgent = new TrapAgent();
            trapAgent.SendV1Trap(new IpAddress(_config.Value.Snmp.SnmpReceiverIp),
                _config.Value.Snmp.SnmpReceiverPort,
                _config.Value.Snmp.SnmpCommunity,
                new Oid(_config.Value.Snmp.EnterpriseOid),
                new IpAddress(_config.Value.Snmp.SnmpAgentIp),
                6,
                (int)trapType, // my trap type 
                12345678, // system UpTime in 0,1sec
                trapData);
            _logger.Info(Logs.DataCenter, "SendSnmpV1Trap sent.");
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(Logs.DataCenter, $"SendSnmpV1Trap: {e.Message}");
            return false;
        }
    }

    public bool SendSnmpV2CTrap(VbCollection trapData, uint upTime, Oid trapObjOid)
    {
        try
        {
            TrapAgent trapAgent = new TrapAgent();
            trapAgent.SendV2Trap(new IpAddress(_config.Value.Snmp.SnmpReceiverIp),
                _config.Value.Snmp.SnmpReceiverPort,
                "public",
                upTime,
                trapObjOid,
                trapData);
            _logger.Info(Logs.DataCenter, "SendSnmpV2Trap sent.");
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(Logs.DataCenter, $"SendSnmpV2Trap: {e.Message}");
            return false;
        }
    }

    public bool SentRealTrap(List<KeyValuePair<FtTrapProperty, string>> data, FtTrapType trapType)
    {
        var trapData = new VbCollection();
        foreach (KeyValuePair<FtTrapProperty, string> pair in data)
        {
            trapData.Add(new Oid(_config.Value.Snmp.EnterpriseOid + "." + (int)pair.Key),
                new OctetString(EncodeString(pair.Value, _config.Value.Snmp.SnmpEncoding)));
        }
        return SendSnmpV1Trap(trapData, trapType);
    }

    private VbCollection CreateTestTrapData()
    {
        var trapData = new VbCollection();

        trapData.Add(new Oid(_config.Value.Snmp.EnterpriseOid + "." + (int)FtTrapProperty.TestString),
            new OctetString(EncodeString("Test string with Русский язык.", _config.Value.Snmp.SnmpEncoding)));
        trapData.Add(new Oid(_config.Value.Snmp.EnterpriseOid + "." + (int)FtTrapProperty.EventRegistrationTime),
            new OctetString(DateTime.Now.ToString("G")));
        trapData.Add(new Oid(_config.Value.Snmp.EnterpriseOid + "." + (int)FtTrapProperty.TestInt), new Integer32(412));
        var doubleValue = 43.0319;
        trapData.Add(new Oid(_config.Value.Snmp.EnterpriseOid + "." + (int)FtTrapProperty.TestDouble),
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