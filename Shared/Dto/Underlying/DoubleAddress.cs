namespace Fibertest.Dto;

public class DoubleAddress
{
    public NetAddress Main { get; set; } = new NetAddress();
    public bool HasReserveAddress { get; set; }
    public NetAddress? Reserve { get; set; }

    public DoubleAddress Clone()
    {
        return new DoubleAddress()
        {
            Main = Main.Clone(),
            HasReserveAddress = HasReserveAddress,
            Reserve = HasReserveAddress ? Reserve!.Clone() : null,
        };
    }
}