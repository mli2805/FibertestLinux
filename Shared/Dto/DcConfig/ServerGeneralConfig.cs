namespace Fibertest.Dto;

public class ServerGeneralConfig
{
    public string ServerTitle { get; set; } = string.Empty;
    public DoubleAddress ServerDoubleAddress { get; set; } = new DoubleAddress();
    public string DatacenterVersion { get; set; } = string.Empty;
    public string PreviousStartOnVersion { get; set; } = string.Empty;
    public int GsmModemComPort { get; set; }
    public bool IsWithoutMapMode { get; set; }
}