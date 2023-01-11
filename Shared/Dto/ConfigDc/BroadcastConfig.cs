
namespace Fibertest.Dto;

public class BroadcastConfig
{
    public string GsmModemComPort { get; set; } = string.Empty;
    public int EventLifetimeLimit { get; set; } = 1800;
}