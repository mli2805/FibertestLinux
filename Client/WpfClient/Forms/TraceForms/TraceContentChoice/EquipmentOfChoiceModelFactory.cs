using System;
using System.Windows;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class EquipmentOfChoiceModelFactory
    {
        public EquipmentOfChoiceModel Create(Equipment equipment)
        {
            var equipmentOfChoiceModel = new EquipmentOfChoiceModel()
            {
                EquipmentId = equipment.EquipmentId,
                TitleOfEquipment = equipment.Title,
                TypeOfEquipment = equipment.Type.ToLocalizedString(),
                LeftCableReserve = equipment.CableReserveLeft,
                RightCableReserve = equipment.CableReserveRight,
                IsRadioButtonEnabled = true,
                RightCableReserveVisible = (equipment.Type > EquipmentType.CableReserve && equipment.Type != EquipmentType.Terminal) ? Visibility.Visible : Visibility.Hidden,
            };
            return equipmentOfChoiceModel;
        }

        public EquipmentOfChoiceModel CreateDoNotUseEquipment(Guid equipmentId, bool isLastNode)
        {
            var doNotUseOptionModel = new EquipmentOfChoiceModel()
            {
                EquipmentId = equipmentId,
                TitleOfEquipment = "",
                TypeOfEquipment = Resources.SID_Do_not_use,
                IsRadioButtonEnabled = !isLastNode,
            };
            return doNotUseOptionModel;
        }
    }
}