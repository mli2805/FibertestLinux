using System;
using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public static class TraceDevReportExt
    {
        public static bool TraceDevReport(this Model readModel, Trace trace, out List<string> content)
        {
            content = new List<string>();
            var flag = true;
            content.Add($@"TraceId = {trace.TraceId}");
            var rtu = readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null)
            {
                content.Add(@"!!! RTU не найден !!!");
                return false;
            }
            content.Add($@"RTU  {rtu.Title}, RtuId = {rtu.Id}");

            for (int i = 1; i < trace.NodeIds.Count; i++)
            {
                var fiberBetweenNodes = readModel.Fibers.FirstOrDefault(
                        f => f.NodeId1 == trace.NodeIds[i - 1] && f.NodeId2 == trace.NodeIds[i] ||
                             f.NodeId1 == trace.NodeIds[i] && f.NodeId2 == trace.NodeIds[i - 1]);
                if (fiberBetweenNodes == null)
                {
                    content.Add($@"        не найдено волокно между узлами {trace.NodeIds[i - 1].First6()} и {trace.NodeIds[i].First6()}");
                    flag = false;
                }
                else if (i - 1 >= trace.FiberIds.Count)
                {
                    content.Add($@"        кончились волокна в списке");
                    flag = false;
                }
                else if (fiberBetweenNodes.FiberId != trace.FiberIds[i - 1])
                {
                    content.Add($@"        !!! В списке волокно {trace.FiberIds[i - 1].First6()}, а между узлами {fiberBetweenNodes.FiberId.First6()}");
                    content.Add(readModel.FiberStr(trace.FiberIds[i - 1]));
                    content.Add(readModel.FiberStr(fiberBetweenNodes.FiberId));
                    flag = false;
                }
                else
                    content.Add(readModel.FiberStr(fiberBetweenNodes.FiberId));

                var node = readModel.Nodes.FirstOrDefault(n => n.NodeId == trace.NodeIds[i]);
                var nodeTitle = node == null ? @"!!! нет такого узла !!!" : node.Title;
                var equipment = readModel.Equipments.FirstOrDefault(e => e.EquipmentId == trace.EquipmentIds[i]);
                if (equipment == null)
                    content.Add($@"не оборудования для узла {nodeTitle} (index {i})");
                var eqStr = equipment != null ? equipment.Type.ToShortString() : "";
                content.Add($@"{i:000} узел {trace.NodeIds[i].First6()}  {eqStr} {nodeTitle}");
            }

            return flag;
        }


        private static string FiberStr(this Model readModel, Guid fiberId)
        {
            var fiber = readModel.Fibers.FirstOrDefault(f => f.FiberId == fiberId);
            var ne = fiber == null ? @"не " : "";
            return $@"        волокно {fiberId.First6()} {ne}найдено";
        }

    }
}
