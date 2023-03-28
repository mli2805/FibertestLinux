using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public static class TraceDevReportExt2
    {
        public static List<string> TraceDevReport2(this Model readModel, Trace trace)
        {
            var content = new List<string>();
            content.Add(@"в предпоследнем столбце: ищем волокно между узлом i и предыдущим");
            content.Add(@"в последнем столбце: пишем волокно из таблицы волокон");
            content.Add(@"");
            content.Add(@"                  узел                         оборудование          в1       в2 ");
            var rtu = readModel.Rtus.First(r => r.Id == trace.RtuId);
            content.Add($@"000 {rtu.Title}");

            for (int i = 1; i < trace.NodeIds.Count; i++)
            {
                var str = $@"{i:000}  {readModel.NodePart(trace, i)}  {readModel.FiberPart(trace, i)}";
                content.Add(str);
            }

            return content;
        }

        private static string NodePart(this Model readModel, Trace trace, int i)
        {
            var node = readModel.Nodes.FirstOrDefault(n => n.NodeId == trace.NodeIds[i]);
            var nodeTitle = node == null ? @"! нет узла !" : (node.Title ?? "").FixedSize(28);
            var nodeStr = $@"{nodeTitle} {trace.NodeIds[i].First6()}   ";

            var equipment = readModel.Equipments.FirstOrDefault(e => e.EquipmentId == trace.EquipmentIds[i]);
            var eqTitle = equipment == null 
                ? @"! нет обор !" 
                : (equipment.Title ?? "").FixedSize(12);
            var eqType = equipment != null ? equipment.Type.ToShortString() : @"! нет обор !";
            return nodeStr + $@"  {eqTitle} {eqType}";
        }

        private static string FiberPart(this Model readModel, Trace trace, int i)
        {
            var fiberBetweenNodes = readModel.Fibers.FirstOrDefault(
                f => f.NodeId1 == trace.NodeIds[i - 1] && f.NodeId2 == trace.NodeIds[i] ||
                     f.NodeId1 == trace.NodeIds[i] && f.NodeId2 == trace.NodeIds[i - 1]);
            var fiberBetween = fiberBetweenNodes != null ? fiberBetweenNodes.FiberId.First6() : @"! нет!";

            string fiberInTableStr;
            if (i - 1 >= trace.FiberIds.Count)
            {
                fiberInTableStr = @"! список кончился !";
            }
            else
            {
                var fiberInTable = readModel.Fibers.FirstOrDefault(f => f.FiberId == trace.FiberIds[i-1]);
                fiberInTableStr = fiberInTable != null ? fiberInTable.FiberId.First6() : @"! нет!";
            }
       
            return $@"{fiberBetween}   {fiberInTableStr}";
        }

        private static string FixedSize(this string str, int size)
        {
            if (str.Length > size)
                return str.Substring(0, size);
            return str.PadRight(size);
        }
    }
}