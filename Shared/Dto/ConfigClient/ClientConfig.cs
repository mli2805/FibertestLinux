namespace Fibertest.Dto;

public class ClientConfig
{
    public ClientGeneralConfig General { get; set; } = new ClientGeneralConfig();
    public MapConfig Map { get; set; } = new MapConfig();
    public OtdrParametersConfig OtdrParameters { get; set; } = new OtdrParametersConfig();
}