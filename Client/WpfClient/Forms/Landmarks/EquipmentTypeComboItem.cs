using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class EquipmentTypeComboItem
    {
        public EquipmentType Type { get; set; }

        public EquipmentTypeComboItem(EquipmentType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type.ToLocalizedString();
        }
    }
}