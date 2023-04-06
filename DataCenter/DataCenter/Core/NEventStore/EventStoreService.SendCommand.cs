using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public partial class EventStoreService
    {
        public async Task<string?> SendCommandWrapped(object cmd, string username, string clientIp)
        {
            if (cmd is RemoveEventsAndSors removeEventsAndSors)
            {
                await Task.Factory.StartNew(() => RemoveEventsAndSors(removeEventsAndSors, username, clientIp));
                return null;
            }

            if (cmd is MakeSnapshot makeSnapshot)
            {
                _logger.Info(Logs.DataCenter, $"{username} from {clientIp} asked to make snapshot");
                await Task.Factory.StartNew(() => MakeSnapshot(makeSnapshot, username, clientIp));
                return null;
            }

            if (cmd is CleanTrace cleanTrace) // only removes sor files, Trace will be cleaned further
            {
                var res = await RemoveSorFiles(cleanTrace.TraceId);
                if (!string.IsNullOrEmpty(res)) return res;
            }

            if (cmd is RemoveTrace removeTrace) // only removes sor files, Trace will be removed further
            {
                var res = await RemoveSorFiles(removeTrace.TraceId);
                if (!string.IsNullOrEmpty(res)) return res;
            }

            var resultInGraph = await SendCommand(cmd, username, clientIp);
            if (resultInGraph != null)
                return resultInGraph;

            await NotifyWebClient(cmd);

            return await PostProcessing(cmd);

        }

        private async Task<string?> PostProcessing(object cmd)
        {
            if (cmd is RemoveRtu removeRtu)
                return await _rtuStationsRepository.RemoveRtuAsync(removeRtu.RtuId);

            #region Base ref amend
            if (cmd is UpdateAndMoveNode updateAndMoveNode)
                return await AmendForTracesWhichUseThisNode(updateAndMoveNode.NodeId);
            if (cmd is UpdateRtu updateRtu)
                return await AmendForTracesFromRtu(updateRtu.RtuId);
            if (cmd is UpdateNode updateNode)
                return await AmendForTracesWhichUseThisNode(updateNode.NodeId);
            if (cmd is MoveNode moveNode)
                return await AmendForTracesWhichUseThisNode(moveNode.NodeId);
            if (cmd is UpdateEquipment updateEquipment)
                return await ProcessUpdateEquipment(updateEquipment.EquipmentId);
            if (cmd is UpdateFiber updateFiber)
                return await ProcessUpdateFiber(updateFiber.Id);
            if (cmd is AddNodeIntoFiber addNodeIntoFiber)
                return await AmendForTracesWhichUseThisNode(addNodeIntoFiber.Id);
            if (cmd is RemoveNode removeNode && removeNode.IsAdjustmentPoint)
                return await ProcessNodeRemoved(removeNode.DetoursForGraph.Select(d => d.TraceId)
                    .ToList());
            #endregion

            return null;
        }
    }
}
