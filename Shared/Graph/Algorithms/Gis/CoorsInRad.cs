namespace Fibertest.Graph
{
    public class CoorsInRad
    {
        public double Lat;
        public double Lng;

        public CoorsInRad()
        {
        }

        public CoorsInRad(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }

        public CoorsInRad(PointLatLng p)
        {
            Lat = p.Lat * Math.PI / 180;
            Lng = p.Lng * Math.PI / 180;
        }
    }
}