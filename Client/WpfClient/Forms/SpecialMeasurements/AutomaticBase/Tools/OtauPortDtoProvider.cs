using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public static class OtauPortDtoProvider
    {
        public static List<OtauPortDto> PreparePairOfOtauPortDto(this Leaf parent, int portNumber, Model readModel)
        {
            var rtuId = (parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent).Id;
            var portOwner = (IPortOwner)parent;
            var rtu = readModel.Rtus.First(r => r.Id == rtuId);
            var otau = readModel.Otaus.FirstOrDefault(o => o.Serial == portOwner.Serial);

            return PrepareOtauPortDto(rtu, otau, portOwner, portNumber);
        }

        private static List<OtauPortDto> PrepareOtauPortDto(Rtu rtu, Otau? otau, IPortOwner otauLeaf, int portNumber)
        {
            var otauId = otau == null
                ? rtu.MainVeexOtau.id
                : otau.Id.ToString();

            var otauPortDto = new OtauPortDto(portNumber, otauLeaf is RtuLeaf)
            {
                OtauId = otauId,
                NetAddress = otau?.NetAddress ?? rtu.MainChannel,
                Serial = otauLeaf.Serial,
                MainCharonPort = otau?.MasterPort ?? 1
            };

            var result = new List<OtauPortDto>() { otauPortDto };

            if (!otauPortDto.IsPortOnMainCharon && rtu.RtuMaker == RtuMaker.VeEX)
            {
                result.Add(new OtauPortDto(otauPortDto.MainCharonPort, true) // Veex requires Main OTAU also
                {
                    OtauId = rtu.MainVeexOtau.id,
                });
            }

            return result;
        }
    }
}