namespace Fibertest.DataCenter;

public class MysqlConfig
{
    public int TcpPort { get; set; } = 3306;
    public string SchemePostfix { get; set; } = "";
    public bool ResetDb { get; set; } = false;
    public int FreeSpaceThresholdGb { get; set; } = 10;
}