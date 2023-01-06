namespace Fibertest.Dto;

public class CharonConfig
{
    public string ComPortName { get; set; } = "/dev/ttyS1";
    public int ComPortSpeed { get; set; } = 115200;

    public int PauseAfterReset { get; set; } = 5;

    public int ConnectionTimeout { get; set; } = 5;
    public int ReadTimeout { get; set; } = 2;
    public int WriteTimeout { get; set; } = 2;
    public int PauseBetweenCommandsMs { get; set; } = 200;
}