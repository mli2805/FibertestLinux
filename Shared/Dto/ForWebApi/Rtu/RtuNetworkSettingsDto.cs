namespace Fibertest.Dto;

public class RtuNetworkSettingsDto
{
    public string? RtuTitle{ get; set; }
    public RtuMaker RtuMaker;
    public string? MainChannel{ get; set; }
    public bool IsReserveChannelSet{ get; set; }
    public string? ReserveChannel{ get; set; }
    public string? OtdrAddress{ get; set; }
    public string? Mfid{ get; set; }
    public string? Serial{ get; set; }
    public int OwnPortCount{ get; set; }
    public int FullPortCount{ get; set; }
    public string? Version{ get; set; }
    public string? Version2{ get; set; }

}