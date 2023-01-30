namespace Fibertest.Dto;

public class ClientConfig
{
    public ClientGeneralConfig General { get; set; } = new ClientGeneralConfig();
    public MapConfig Map { get; set; } = new MapConfig();
    public OtdrParametersConfig OtdrParameters { get; set; } = new OtdrParametersConfig();

    public ClientMiscellaneousConfig Miscellaneous { get; set; } = new ClientMiscellaneousConfig();
    public CharonConfig CharonConfig { get; set; } = new CharonConfig();

    public List<ServerForClient> ServerList { get; set; } = new List<ServerForClient>();
}

