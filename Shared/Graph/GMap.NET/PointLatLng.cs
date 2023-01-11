using System.Globalization;

namespace Fibertest.Graph;

public struct PointLatLng
{
    public double Lat;
    public double Lng;

    public PointLatLng(double lat, double lng)
    {
        Lat = lat;
        Lng = lng;
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.CurrentCulture, "{{Lat={0}, Lng={1}}}", this.Lat, this.Lng);
    }
}