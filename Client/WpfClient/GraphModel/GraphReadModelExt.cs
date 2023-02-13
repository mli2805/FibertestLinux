using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Fibertest.Dto;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public static class GraphReadModelExt
    {
       
        public static FiberVm GetAnotherFiberOfAdjustmentPoint(this GraphReadModel model, NodeVm adjustmentPoint, Guid fiberId)
        {
            return model.Data.Fibers.First(f => (f.Node1.Id == adjustmentPoint.Id || f.Node2.Id == adjustmentPoint.Id) && f.Id != fiberId);
        }

        public static void CleanAccidentPlacesOnTrace(this GraphReadModel model, Guid traceId)
        {
            var nodeVmsIndexes = new List<int>();
            for (int i = 0; i < model.Data.Nodes.Count; i++)
            {
                if (model.Data.Nodes[i].AccidentOnTraceVmId == traceId)
                    nodeVmsIndexes.Add(i);
            }

            for (int i = nodeVmsIndexes.Count - 1; i >= 0; i--)
            {
                model.Data.Nodes.RemoveAt(nodeVmsIndexes[i]);
            }

            model.Logger.LogInfo(Logs.Client,$@"{nodeVmsIndexes.Count} accident nodes were cleaned");

            foreach (var fiberVm in model.Data.Fibers)
            {
                fiberVm.RemoveBadSegment(traceId);
            }
        }

        public static Guid ChooseEquipmentForNode(this GraphReadModel model, Guid nodeId, bool isLastNode, out string? dualName)
        {
            dualName = null;
            var node = model.ReadModel.Nodes.First(n => n.NodeId == nodeId);
            var nodeVm = model.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (nodeVm != null)
                nodeVm.IsHighlighted = true;

            var allEquipmentInNode = model.ReadModel.Equipments.Where(e => e.NodeId == nodeId).ToList();

            if (allEquipmentInNode.Count == 1 && allEquipmentInNode[0].Type == EquipmentType.AdjustmentPoint)
                return allEquipmentInNode[0].EquipmentId;

            if (allEquipmentInNode.Count == 1 && !string.IsNullOrEmpty(node.Title))
            {
                dualName = node.Title;
                var equipment =
                    model.ReadModel.Equipments.First(e => e.EquipmentId == allEquipmentInNode[0].EquipmentId);
                if (!string.IsNullOrEmpty(equipment.Title))
                    dualName = dualName + @" / " + equipment.Title;
                return allEquipmentInNode[0].EquipmentId;
            }

            var traceContentChoiceViewModel = model.GlobalScope.Resolve<TraceContentChoiceViewModel>();
            traceContentChoiceViewModel.Initialize(allEquipmentInNode, node, isLastNode);
            model.WindowManager.ShowDialogWithAssignedOwner(traceContentChoiceViewModel).Wait();
            model.ExtinguishAllNodes();
            if (!traceContentChoiceViewModel.ShouldWeContinue) // user left the process
                return Guid.Empty;

            var selectedEquipmentGuid = traceContentChoiceViewModel.GetSelectedEquipmentGuid();
            dualName = traceContentChoiceViewModel.GetSelectedDualName();
            return selectedEquipmentGuid;

        }
    }
}