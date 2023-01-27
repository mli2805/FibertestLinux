using Fibertest.Dto;

namespace Fibertest.Graph
{
    public class LandmarksGraphParser
    {
        private readonly Model _readModel;

        public LandmarksGraphParser(Model readModel)
        {
            _readModel = readModel;
        }

        public List<Landmark> GetLandmarks(Trace trace)
        {
            var previousNode = _readModel.Nodes.First(n => n.NodeId == trace.NodeIds[0]);
            var result = new List<Landmark> { CreateRtuLandmark(previousNode) };

            var distance = 0.0;
            var j = 1;
            for (var i = 1; i < trace.NodeIds.Count; i++)
            {
                var nodeId = trace.NodeIds[i];
                var node = _readModel.Nodes.First(n => n.NodeId == nodeId);
                distance = distance + GisLabCalculator.GetDistanceBetweenPointLatLng(previousNode.Position, node.Position) / 1000;
                previousNode = node;
                if (node.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint) continue;

                var lm = CreateLandmark(node, trace.EquipmentIds[i], j++, i);
                lm.Distance = distance;
                result.Add(lm);
            }

            return result;
        }

        private Landmark CreateLandmark(Node node, Guid equipmentId, int number, int numberIncludingAdjustmentPoints)
        {
            var equipment = _readModel.Equipments.First(e => e.EquipmentId == equipmentId);
            var comment = number == 0
                ? _readModel.Rtus.First(r => r.NodeId == node.NodeId).Comment
                : node.Comment;
            return new Landmark()
            {
                Number = number,
                NumberIncludingAdjustmentPoints = numberIncludingAdjustmentPoints,
                NodeId = node.NodeId,
                NodeTitle = node.Title,
                NodeComment = comment,
                EquipmentId = equipmentId,
                EquipmentTitle = equipment.Title,
                EquipmentType = equipment.Type,
                EventNumber = -1,
                GpsCoors = node.Position,
            };
        }

        private Landmark CreateRtuLandmark(Node node)
        {
            var rtu = _readModel.Rtus.First(e => e.NodeId == node.NodeId);
            return new Landmark()
            {
                Number = 0,
                NodeId = rtu.NodeId,
                NodeTitle = rtu.Title,
                NodeComment = rtu.Comment,
                EquipmentType = EquipmentType.Rtu,
                Distance = 0,
                EventNumber = -1,
                GpsCoors = node.Position,
            };
        }
    }
}