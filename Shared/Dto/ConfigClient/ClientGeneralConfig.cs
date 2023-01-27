namespace Fibertest.Dto;

public class ClientGeneralConfig
{
    public string Version { get; set; } = "3.0.0.0";
    public string Culture { get; set; } = "ru-RU";
    public DoubleAddress ServerAddress { get; set; } = new DoubleAddress() { Main = new NetAddress("192.168.96.21", 11937) };
    public string ServerTitle { get; set; } = String.Empty;

    public NetAddress ClientLocalAddress { get; set; } = new NetAddress("192.168.96.21", 0);
    public int ClientOrdinal { get; set; } = 0;

    public int ClientPollingRateMs { get; set; } = 500;
    public int ClientHeartbeatRateMs { get; set; } = 1000;
    public int FailedPollsLimit { get; set; } = 7;

    public int MysqlTcpPort { get; set; } = 3306; // for Kadastr
}