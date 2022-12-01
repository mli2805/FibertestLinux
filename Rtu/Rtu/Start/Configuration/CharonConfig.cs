namespace Fibertest.Rtu;

public class CharonConfig
{
    public string? ComPortName { get; set; }
    public int ComPortSpeed { get; set; }

    public int PauseAfterReset { get; set; }

    public int ConnectionTimeout { get; set; }
    public int ReadTimeout { get; set; }
    public int WriteTimeout { get; set; }
    public int PauseBetweenCommandsMs { get; set; }
}