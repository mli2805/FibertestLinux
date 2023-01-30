using System;
using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class GrmEquipmentRequests
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly AddEquipmentIntoNodeBuilder _addEquipmentIntoNodeBuilder;

        public GrmEquipmentRequests(IWcfServiceDesktopC2D c2DWcfManager, AddEquipmentIntoNodeBuilder addEquipmentIntoNodeBuilder)
        {
            _c2DWcfManager = c2DWcfManager;
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
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task AddEquipmentIntoNode(RequestAddEquipmentIntoNode request)
        {
            var cmd = await _addEquipmentIntoNodeBuilder.BuildCommand(request.NodeId);
            if (cmd == null)
                return;
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task UpdateEquipment(UpdateEquipment cmd)
        {
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task RemoveEquipment(RemoveEquipment cmd)
        {
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }


    }
}
