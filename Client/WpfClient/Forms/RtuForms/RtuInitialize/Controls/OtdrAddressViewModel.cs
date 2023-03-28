using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    
    public class OtdrAddressViewModel : PropertyChangedBase
    {
        private int _port;

        private string _otdrAddress = null!;
        public string OtdrAddress
        {
            get => _otdrAddress;
            set
            {
                if (value == _otdrAddress) return;
                _otdrAddress = value;
                NotifyOfPropertyChange();
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                if (value == _port) return;
                _port = value;
                NotifyOfPropertyChange();
            }
        }

        public void FromRtu(Rtu rtu)
        {
            OtdrAddress = rtu.OtdrNetAddress.IsAddressSetAsIp
                ? rtu.OtdrNetAddress.Ip4Address == @"192.168.88.101" // fake address on screen
                    ? rtu.MainChannel.Ip4Address
                    : rtu.OtdrNetAddress.Ip4Address
                : rtu.OtdrNetAddress.HostName;
            Port = rtu.OtdrNetAddress.Port;
        }
    }
}
