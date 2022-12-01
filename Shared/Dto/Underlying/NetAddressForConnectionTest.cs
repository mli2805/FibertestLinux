namespace Fibertest.Dto
{
    public class NetAddressForConnectionTest
    {
        public NetAddress Address;
        public bool IsRtuAddress;

        public NetAddressForConnectionTest(NetAddress address, bool isRtuAddress)
        {
            Address = address;
            IsRtuAddress = isRtuAddress;
        }
    }
}