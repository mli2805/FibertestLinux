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
            var rtuLandmark = CreateRtuLandmark(previousNode);
            rtuLandmark.FiberId = Guid.Empty;
            var result = new List<Landmark> { rtuLandmark };

            var distance = 0.0;
            var j = 1;
            for (var i = 1; i < trace.NodeIds.Count; i++)
            {
                var node = _readModel.Nodes.First(n => n.NodeId == trace.NodeIds[i]);
                var fiber = _readModel.Fibers.First(f => f.FiberId == trace.FiberIds[i-1]);
                var section = fiber.UserInputedLength > 0
                    ? fiber.UserInputedLength
                    : GisLabCalculator.GetDistanceBetweenPointLatLng(previousNode.Position, node.Position) / 1000;
                distance += section;
                previousNode = node;
                if (node.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint) continue;

                var lm = CreateLandmark(node, trace.EquipmentIds[i], j++, i);
                lm.FiberId = fiber.FiberId;
                lm.GpsDistance = distance;
                lm.GpsSection = section;
                lm.OpticalDistance = 0.0;
                lm.OpticalSection = 0.0;
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
                IsFromBase = false,
                Number = number,
                NumberIncludingAdjustmentPoints = numberIncludingAdjustmentPoints,
                NodeId = node.NodeId,
                NodeTitle = node.Title,
                NodeComment = comment,
                EquipmentId = equipmentId,
                EquipmentTitle = equipment.Title,
                EquipmentType = equipment.Type,
                EventNumber = -1,
                LeftCableReserve = equipment.CableReserveLeft,
                RightCableReserve = equipment.CableReserveRight,
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
                OpticalDistance = 0,
                EventNumber = -1,
                GpsCoors = node.Position,
            };
        }
    }
}