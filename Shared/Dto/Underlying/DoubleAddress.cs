namespace Fibertest.Dto;

public class DoubleAddress
{
    public NetAddress Main = new NetAddress();
    public bool HasReserveAddress;
    public NetAddress Reserve = new NetAddress();

    public DoubleAddress Clone()
    {
        return new DoubleAddress()
        {
            Main = Main.Clone(),
            HasReserveAddress = HasReserveAddress,
            Reserve = Reserve.Clone(),
        };
    }
}