namespace Fibertest.DataCenter;

public class SnmpConfig
{
    public bool IsSnmpOn { get; set; }
    public string SnmpTrapVersion { get; set; } = "v1?";
    public string SnmpReceiverIp { get; set; } = "192.168.96.21";
    public int SnmpReceiverPort { get; set; } = 162;
    public string SnmpAgentIp { get; set; } = "127.0.0.1";
    public string SnmpCommunity { get; set; } = "IIT";
    public string SnmpEncoding { get; set; } = "windows1251";
    public string EnterpriseOid { get; set; } = "1.3.6.1.4.1.36220";
}