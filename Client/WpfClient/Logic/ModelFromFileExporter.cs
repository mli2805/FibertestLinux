using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class ModelFromFileExporter
    {
        private readonly Model _readModel;
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;

        public ModelFromFileExporter(Model readModel, GrpcC2DRequests grpcC2DRequests, IWcfServiceDesktopC2D c2DWcfManager)
        {
            _readModel = readModel;
            _grpcC2DRequests = grpcC2DRequests;
            _c2DWcfManager = c2DWcfManager;
        }

        public async Task Apply(Model oneRtuModel)
        {
            var rtu = oneRtuModel.Rtus.First();
            var rtuNode = oneRtuModel.Nodes.First(n => n.NodeId == rtu.NodeId);
            var cmd = new AddRtuAtGpsLocation(rtu.Id, rtu.NodeId, rtuNode.Position.Lat, rtuNode.Position.Lng,
                rtu.Title);
            var unused3 = await _grpcC2DRequests.SendEventSourcingCommand(cmd);
            await Task.Delay(2000);
            var initializeRtu = new InitializeRtu()
            {
                Id = rtu.Id,
                Maker = RtuMaker.IIT,
                FullPortCount = rtu.FullPortCount,
                OwnPortCount = rtu.OwnPortCount,
                Serial = rtu.Serial,
                OtauNetAddress = rtu.OtdrNetAddress
            };
            var unused2 = await _grpcC2DRequests.SendEventSourcingCommand(initializeRtu);

            foreach (var otau in oneRtuModel.Otaus)
            {
                var attachOtau = new AttachOtau()
                {
                    Id = otau.Id,
                    RtuId = rtu.Id,
                    MasterPort = otau.MasterPort,
                    PortCount = otau.PortCount,
                    NetAddress = otau.NetAddress,
                    Serial = otau.Serial,
                    IsOk = otau.IsOk
                };
                var unused = await _grpcC2DRequests.SendEventSourcingCommand(attachOtau);
            }

            var commandList = new List<object>();

            foreach (var node in oneRtuModel.Nodes)
            {
                if (_readModel.Nodes.Any(n => n.NodeId == node.NodeId)) continue;

                commandList.Clear();
                var addEquipmentAtGpsLocation = new AddEquipmentAtGpsLocation(node.TypeOfLastAddedEquipment,
                    node.Position.Lat, node.Position.Lng)
                {
                    NodeId = node.NodeId,
                    EmptyNodeEquipmentId = Guid.Empty,
                    RequestedEquipmentId = Guid.Empty,
                };
                commandList.Add(addEquipmentAtGpsLocation);

                var updateNode = new UpdateNode() { NodeId = node.NodeId, Title = node.Title };
                commandList.Add(updateNode);
                var _ = await _c2DWcfManager.SendCommandsAsObjs(commandList);
            }

            foreach (var equipment in oneRtuModel.Equipments)
            {
                if (_readModel.Equipments.Any(n => n.EquipmentId == equipment.EquipmentId))
                    continue;

                commandList.Clear();
                var eqNode = _readModel.Nodes.First(n => n.NodeId == equipment.NodeId);
                var addEquipmentIntoNode = new AddEquipmentIntoNode()
                {
                    EquipmentId = equipment.EquipmentId,
                    Title = equipment.Title,
                    NodeId = eqNode.NodeId,
                    Type = equipment.Type
                };
                commandList.Add(addEquipmentIntoNode);
                var _ = await _c2DWcfManager.SendCommandsAsObjs(commandList);
            }

            foreach (var fiber in oneRtuModel.Fibers)
            {
                if (_readModel.Fibers.Any(n => n.FiberId == fiber.FiberId)) continue;

                commandList.Clear();
                var addFiber = new AddFiber(fiber.NodeId1, fiber.NodeId2)
                {
                    FiberId = fiber.FiberId,
                };
                commandList.Add(addFiber);
                var _ = await _c2DWcfManager.SendCommandsAsObjs(commandList);
            }

            foreach (var trace in oneRtuModel.Traces)
            {
                commandList.Clear();
                var addTrace = new AddTrace(trace.TraceId, trace.RtuId,trace.NodeIds,trace.EquipmentIds,trace.FiberIds)
                {
                    Title = trace.Title,
                    Comment = trace.Comment,
                };
                commandList.Add(addTrace);

                if (trace.IsAttached)
                {
                    var attachTrace = new AttachTrace(trace.TraceId, trace.OtauPort);
                    commandList.Add(attachTrace);
                }
                var _ = await _c2DWcfManager.SendCommandsAsObjs(commandList);
            }

        }
    }
}