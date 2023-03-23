using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class PortsAndBopsViewModel : PropertyChangedBase
    {
        private int _fullPortCount;
        public int FullPortCount
        {
            get => _fullPortCount;
            set
            {
                if (value == _fullPortCount) return;
                _fullPortCount = value;
                NotifyOfPropertyChange();
            }
        }

        private List<string> _bops = null!;
        public List<string> Bops
        {
            get => _bops;
            set
            {
                if (Equals(value, _bops)) return;
                _bops = value;
                NotifyOfPropertyChange();
            }
        }

        public void FillInPortsAndBops(Rtu rtu)
        {
            FullPortCount = rtu.FullPortCount - rtu.Children.Count;
            Bops = FillInOtauList(rtu.Children);
        }

        public void FillInPortsAndBops(Rtu originalRtu, RtuInitializedDto dto)
        {
            if (originalRtu.Serial != dto.Serial) // first initialization or RTU was changed
            {
                FullPortCount = dto.FullPortCount - dto.Children.Count;
            }
            else // re-initialization of the same RTU
            {
                FullPortCount = originalRtu.FullPortCount - originalRtu.Children.Count;
            }

            Bops = FillInOtauList(dto.Children);
        }

        private List<string> FillInOtauList(Dictionary<int, OtauDto>? otaus)
        {
            var bops = new List<string>();
            if (otaus != null)
                foreach (var pair in otaus.OrderBy(p=>p.Key))
                {
                    bops.Add(string.Format(Resources.SID____on_port__0___optical_switch__1___,
                        pair.Key, pair.Value.NetAddress.ToStringA()));

                    var bopSerial = pair.Value.Serial?.Substring(0, pair.Value.Serial.Length - 1) ?? Resources.SID_Not_available;
                    var secondString = pair.Value.IsOk 
                        ? string.Format(Resources.SID_______________________serial__0____1__ports, bopSerial, pair.Value.OwnPortCount)
                        : @"                       " + Resources.SID_Not_available;
                    bops.Add(secondString);
                }
            return bops;
        }
    }
}
