using System;
using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class GrmEquipmentRequests
    {
        private readonly GrpcC2DService _grpcC2DService;
        private readonly AddEquipmentIntoNodeBuilder _addEquipmentIntoNodeBuilder;

        public GrmEquipmentRequests(GrpcC2DService grpcC2DService, AddEquipmentIntoNodeBuilder addEquipmentIntoNodeBuilder)
        {
            _grpcC2DService = grpcC2DService;
            _addEquipmentIntoNodeBuilder = addEquipmentIntoNodeBuilder;
        }

        public async Task AddEquipmentAtGpsLocation(RequestAddEquipmentAtGpsLocation request)
        {
            var cmd = new AddEquipmentAtGpsLocation(request.Type, request.Latitude, request.Longitude)
            {
                RequestedEquipmentId = Guid.NewGuid(),
                NodeId = Guid.NewGuid(),
            };
            cmd.EmptyNodeEquipmentId = request.Type == EquipmentType.EmptyNode || request.Type == EquipmentType.AdjustmentPoint ? Guid.Empty : Guid.NewGuid();
            await _grpcC2DService.SendEventSourcingCommand(cmd);
        }

        public async Task AddEquipmentIntoNode(RequestAddEquipmentIntoNode request)
        {
            var cmd = await _addEquipmentIntoNodeBuilder.BuildCommand(request.NodeId);
            if (cmd == null)
                return;
            await _grpcC2DService.SendEventSourcingCommand(cmd);
        }

        public async Task UpdateEquipment(UpdateEquipment cmd)
        {
            await _grpcC2DService.SendEventSourcingCommand(cmd);
        }

        public async Task RemoveEquipment(RemoveEquipment cmd)
        {
            await _grpcC2DService.SendEventSourcingCommand(cmd);
        }


    }
}
