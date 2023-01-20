namespace Fibertest.Dto;

public class ClientGeneralConfig
{
    public string Version { get; set; } = "3.0.0.0";
    public string Culture { get; set; } = "ru-RU";
    public DoubleAddress ServerAddress { get; set; } = new DoubleAddress() { Main = new NetAddress("192.168.96.21", 11937) };
    public int MysqlTcpPort { get; set; } = 3306; // for Kadastr
}