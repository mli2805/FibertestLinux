namespace Fibertest.Dto;

public class OtauPortDto
{
    public string? OtauId; // in VeEX RTU main OTAU has its own ID, for MAK it is a RTU ID
    public NetAddress NetAddress = new NetAddress();
    public int OpticalPort;
    public string? Serial;
    public bool IsPortOnMainCharon;
    public int MainCharonPort; // only for additional otau - port of main otau this otau is connected to

    public OtauPortDto Clone()
    {
        var clone = (OtauPortDto)MemberwiseClone();
        clone.NetAddress = NetAddress.Clone();
        return clone;
    }

    public string ToStringB()
    {
        return IsPortOnMainCharon ? OpticalPort.ToString() : $"{MainCharonPort}:{OpticalPort}";
    }
}