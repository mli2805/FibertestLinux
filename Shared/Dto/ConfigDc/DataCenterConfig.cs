namespace Fibertest.Dto;

public class DataCenterConfig
{
    public ServerGeneralConfig General { get; set; } = new ServerGeneralConfig();
    public ServerTimeoutConfig ServerTimeouts { get; set; } = new ServerTimeoutConfig();
    public EventSourcingConfig EventSourcing { get; set; } = new EventSourcingConfig();
    public MysqlConfig Mysql { get; set; } = new MysqlConfig();
    public BroadcastConfig Broadcast { get; set; } = new BroadcastConfig();
    public SmtpConfig Smtp { get; set; } = new SmtpConfig();
    public SnmpConfig Snmp { get; set; } = new SnmpConfig();
    public WebApiConfig WebApi { get; set; } = new WebApiConfig();
}