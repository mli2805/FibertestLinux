namespace Fibertest.Dto;

public class SnmpSettingsDto
{
    public bool IsSnmpOn;
    public string? SnmpTrapVersion;
    public string? SnmpReceiverIp;
    public int SnmpReceiverPort;
    public string? SnmpAgentIp;
    public string? SnmpCommunity;
    public string? EnterpriseOid;
    public string? SnmpEncoding;
}