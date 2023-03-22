using Fibertest.Dto;
using GMap.NET;

namespace Fibertest.Graph
{
    public class Landmark
    {
        public bool IsFromBase { get; set; }
        public int Number { get; set; }
        public int NumberIncludingAdjustmentPoints { get; set; }
        public Guid NodeId { get; set; }
        public Guid FiberId { get; set; } // to the left
        public string? NodeTitle { get; set; }
        public string? NodeComment { get; set; }
        public Guid EquipmentId { get; set; }
        public string? EquipmentTitle { get; set; }
        public EquipmentType EquipmentType { get; set; }
        public int LeftCableReserve { get; set; }
        public int RightCableReserve { get; set; }
        public double GpsDistance { get; set; }
        public double GpsSection { get; set; }
        public bool IsUserInput { get; set; }
        public double OpticalDistance { get; set; }
        public double OpticalSection { get; set; }
        public int EventNumber { get; set; }
        public PointLatLng GpsCoors { get; set; }
        
        public Landmark Clone()
        {
            return new Landmark()
            {
                Number = Number,
                NumberIncludingAdjustmentPoints = NumberIncludingAdjustmentPoints,
                NodeId = NodeId,
                FiberId = FiberId,
                NodeTitle = NodeTitle,
                NodeComment = NodeComment,
                EquipmentId = EquipmentId,
                EquipmentTitle = EquipmentTitle,
                EquipmentType = EquipmentType,
                LeftCableReserve = LeftCableReserve,
                RightCableReserve = RightCableReserve,
                GpsDistance = GpsDistance,
                GpsSection = GpsSection,
                OpticalDistance = OpticalDistance,
                OpticalSection = OpticalSection,
                EventNumber = EventNumber,
                GpsCoors = new PointLatLng(GpsCoors.Lat, GpsCoors.Lng),
            };
        }
    }
}