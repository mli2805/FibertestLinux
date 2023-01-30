using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public static class GrmAddTraceExt
    {
        public static async Task AddTrace(this GraphReadModel model, RequestAddTrace request)
        {
            if (!model.Validate(request))
                return;
        
            List<Guid> traceNodes = await model.GetPath(request);
            if (traceNodes == null)
                return;
        
            var traceId = Guid.NewGuid();
            var fiberIds = model.ReadModel.GetFibersAtTraceCreation(traceNodes).ToList();
            if (fiberIds.Count + 1 != traceNodes.Count)
            {
                var errVm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Nodes_count_does_not_match_sections_count_);
                model.WindowManager.ShowDialogWithAssignedOwner(errVm);
                return;
            }
            model.SetFutureTraceLightOnOff(traceId, fiberIds, true);
            try
            {
                var vm = new MyMessageBoxViewModel(MessageType.Confirmation, Resources.SID_Accept_the_path);
                model.WindowManager.ShowDialogWithAssignedOwner(vm);
        
                if (!vm.IsAnswerPositive) return;
        
                List<Guid> traceEquipments = model.CollectEquipment(traceNodes);
        
                if (traceEquipments == null)
                    return;
        
                var traceAddViewModel = model.GlobalScope.Resolve<TraceInfoViewModel>();
                await traceAddViewModel.Initialize(traceId, traceEquipments, traceNodes, true);
                model.WindowManager.ShowDialogWithAssignedOwner(traceAddViewModel);
                // if (traceAddViewModel.IsCreatedSuccessfully)
                    // return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                model.SetFutureTraceLightOnOff(traceId, fiberIds, false);
            }
        }

        private static bool Validate(this GraphReadModel model, RequestAddTrace request)
        {
            if (model.ReadModel.Equipments.Any(e => e.NodeId == request.LastNodeId && e.Type > EquipmentType.CableReserve)) return true;

            model.WindowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Last_node_of_trace_must_contain_some_equipment));
            return false;
        }

        private static async Task<List<Guid>> GetPath(this GraphReadModel graphReadModel, RequestAddTrace request)
        {
            var vm = new WaitViewModel();
            vm.Initialize(LongOperation.PathFinding);
            graphReadModel.WindowManager.ShowWindowWithAssignedOwner(vm);

            var pathFinder = new PathFinder(graphReadModel.ReadModel);
            var path = await Task.Factory.StartNew(() => pathFinder.FindPath(request.NodeWithRtuId, request.LastNodeId).Result);

            vm.TryCloseAsync();

            if (path == null)
            {
                var strs = new List<string>()
                {
                    Resources.SID_Path_couldn_t_be_found,
                    "",
                    Resources.SID_Load_additional_data_,
                };
                graphReadModel.WindowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, strs));
            }

            return path;
        }


        public static List<Guid> CollectEquipment(this GraphReadModel model, List<Guid> nodes)
        {
            var equipments = new List<Guid> { model.ReadModel.Rtus.First(r => r.NodeId == nodes[0]).Id };
            foreach (var nodeId in nodes.Skip(1))
            {
                var equipmentId = model.ChooseEquipmentForNode(nodeId, nodeId == nodes.Last(), out var _);
                if (equipmentId == Guid.Empty)
                    return null;
                equipments.Add(equipmentId);
            }
            return equipments;
        }

      

    }
}