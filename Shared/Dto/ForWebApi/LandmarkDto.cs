namespace Fibertest.Dto
{
    public class LandmarkDto
    {
        public int Ordinal;
        public string? NodeTitle;
        public EquipmentType EquipmentType;
        public string? EquipmentTitle;

        public string? CableReserves;
        public string? GpsDistance;
        public string? GpsSection;
        public bool IsUserInput;
        public string? OpticalDistance;
        public string? OpticalSection;

        public int EventOrdinal;
        public GeoPoint? Coors;
    }
}