namespace Fibertest.DataCenter;

public class MysqlConfig
{
    public int TcpPort { get; set; }
    public string? SchemePostfix { get; set; }
    public bool ResetDb { get; set; }
    public int FreeSpaceThresholdGb { get; set; }
}