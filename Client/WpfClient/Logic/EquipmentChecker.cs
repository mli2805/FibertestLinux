using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public static class EquipmentChecker48
    {

        public static async Task<bool> EquipmentCanBeChanged(this Model readModel, Guid equipmentId, IWindowManager windowManager)
        {
            foreach (var trace in readModel.Traces)
            {
                if (trace.EquipmentIds.Contains(equipmentId) && trace.IsIncludedInMonitoringCycle)
                {
                    var rtu = readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
                    if (rtu != null && rtu.MonitoringState == MonitoringState.On)
                    {
                        await windowManager.ShowDialogWithAssignedOwner(
                            new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                            {
                                Resources.SID_This_equipment_could_not_be_changed_,
                                "",
                                Resources.SID_There_are_traces_which_use_this_equipment_and_are_under_monitoring_now_,
                                Resources.SID_Stop_monitoring_in_order_to_change_equipment
                            }, 0));
                         
                        return false;
                    }
                }
            }
            return true;
        }
    }
}