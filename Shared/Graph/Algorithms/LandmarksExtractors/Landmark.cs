using Fibertest.Dto;
using GMap.NET;

namespace Fibertest.Graph
{
    public class Landmark : ICloneable
    {
        public int Number { get; set; }
        public int NumberIncludingAdjustmentPoints { get; set; }
        public Guid NodeId { get; set; }
        public string NodeTitle { get; set; }
        public string NodeComment { get; set; }
        public Guid EquipmentId { get; set; }
        public string EquipmentTitle { get; set; }
        public EquipmentType EquipmentType { get; set; }
        public double Distance { get; set; }
        public int EventNumber { get; set; }
        public PointLatLng GpsCoors { get; set; }
        
        public object Clone()
        {
            return new Landmark()
            {
                Number = Number,
                NumberIncludingAdjustmentPoints = NumberIncludingAdjustmentPoints,
                NodeId = NodeId,
                NodeTitle = NodeTitle,
                NodeComment = NodeComment,
                EquipmentId = EquipmentId,
                EquipmentTitle = EquipmentTitle,
                EquipmentType = EquipmentType,
                Distance = Distance,
                EventNumber = EventNumber,
                GpsCoors = new PointLatLng(GpsCoors.Lat, GpsCoors.Lng),
            };
        }
    }
}